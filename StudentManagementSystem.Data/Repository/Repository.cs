using ClosedXML.Excel;
using StudentManagementSystem.Data.Entities;
using StudentManagementSystem.Data.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StudentManagementSystem.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly string _excelFile = Path.Combine(Directory.GetCurrentDirectory(), "Excel", "StudentData.xlsx");
        private readonly object _fileLock = new object();
        private const string DateOnlyFormat = "yyyy-MM-dd";

        public Repository()
        {
            EnsureExcelFile();
        }

        private void EnsureExcelFile()
        {
            var dir = Path.GetDirectoryName(_excelFile)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_excelFile))
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.AddWorksheet("Sheet1");
                // create header from T properties (ordered by name)
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .OrderBy(p => p.Name).ToArray();
                for (int i = 0; i < props.Length; i++)
                    sheet.Cell(1, i + 1).Value = props[i].Name;
                workbook.SaveAs(_excelFile);
            }
        }

        private PropertyInfo[] GetPropertiesAll()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        private void WriteToExcel(List<T> items)
        {
            lock (_fileLock)
            {
                var props = GetPropertiesAll().OrderBy(p => p.Name).ToArray();
                using var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Sheet1");

                // Header
                for (int i = 0; i < props.Length; i++)
                    worksheet.Cell(1, i + 1).Value = props[i].Name;

                // Rows
                for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
                {
                    var item = items[rowIndex];
                    for (int colIndex = 0; colIndex < props.Length; colIndex++)
                    {
                        var prop = props[colIndex];
                        var val = prop.GetValue(item);
                        if (val is DateOnly d)
                            worksheet.Cell(rowIndex + 2, colIndex + 1).Value = d.ToString(DateOnlyFormat);
                        else if (val is DateTime dt)
                            worksheet.Cell(rowIndex + 2, colIndex + 1).Value = dt.ToString("o");
                        else
                            worksheet.Cell(rowIndex + 2, colIndex + 1).Value = val?.ToString() ?? "";
                    }
                }

                workbook.SaveAs(_excelFile);
            }
        }

        public List<T> GetAll()
        {
            lock (_fileLock)
            {
                var list = new List<T>();
                if (!File.Exists(_excelFile))
                    return list;

                using var workbook = new XLWorkbook(_excelFile);
                var sheet = workbook.Worksheet(1);

                var headerRow = sheet.Row(1);
                var headerCells = headerRow.CellsUsed();
                if (headerCells == null)
                    return list;

                // Map column index -> PropertyInfo using header text
                var props = GetPropertiesAll();
                var colIndexToProp = new Dictionary<int, PropertyInfo>();
                foreach (var cell in headerCells)
                {
                    var headerName = cell.GetString().Trim();
                    if (string.IsNullOrEmpty(headerName))
                        continue;

                    var prop = props.FirstOrDefault(p => string.Equals(p.Name, headerName, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                        colIndexToProp[cell.Address.ColumnNumber] = prop;
                    // else: unknown header — ignored
                }

                // Read rows
                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    var obj = new T();
                    foreach (var kvp in colIndexToProp)
                    {
                        var col = kvp.Key;
                        var prop = kvp.Value;
                        var cell = row.Cell(col);
                        var cellValue = cell?.GetString() ?? string.Empty;
                        if (string.IsNullOrEmpty(cellValue))
                            continue;

                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        try
                        {
                            object? safeValue = null;

                            if (targetType == typeof(string))
                            {
                                safeValue = cellValue;
                            }
                            else if (targetType == typeof(int))
                            {
                                if (int.TryParse(cellValue, out var i)) safeValue = i;
                            }
                            else if (targetType == typeof(long))
                            {
                                if (long.TryParse(cellValue, out var l)) safeValue = l;
                            }
                            else if (targetType == typeof(bool))
                            {
                                if (bool.TryParse(cellValue, out var b)) safeValue = b;
                            }
                            else if (targetType == typeof(DateOnly))
                            {
                                if (DateOnly.TryParseExact(cellValue, DateOnlyFormat, null, System.Globalization.DateTimeStyles.None, out var dd))
                                    safeValue = dd;
                                else if (DateOnly.TryParse(cellValue, out dd))
                                    safeValue = dd;
                                else
                                {
                                    // try parsing as DateTime then convert
                                    if (DateTime.TryParse(cellValue, out var dto))
                                        safeValue = DateOnly.FromDateTime(dto);
                                }
                            }
                            else if (targetType == typeof(DateTime))
                            {
                                if (DateTime.TryParse(cellValue, out var dto)) safeValue = dto;
                            }
                            else
                            {
                                // fallback - attempt Convert.ChangeType inside try/catch
                                try { safeValue = Convert.ChangeType(cellValue, targetType); }
                                catch { safeValue = null; }
                            }

                            if (safeValue != null)
                                prop.SetValue(obj, safeValue);
                            // If parsing failed we skip setting the property (leave default) — avoids crashes
                        }
                        catch (Exception ex)
                        {
                            // Don't throw; log context and continue (prevents whole request failing).
                            Console.WriteLine($"Repository<{typeof(T).Name}>: failed to set property '{prop.Name}' from cell [{row.RowNumber()}, {col}] value='{cellValue}': {ex.Message}");
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
        }

        public bool Any(Expression<Func<T, bool>> predicate) => GetAll().AsQueryable().Any(predicate);

        public T Create(T entity)
        {
            var list = GetAll();

            var idProp = typeof(T).GetProperty("ID");
            long id = 1;
            if (list.Any())
            {
                var existingMax = list.Max(e => (long)(idProp?.GetValue(e) ?? 0));
                id = existingMax + 1;
            }

            var incomingId = (long)(idProp?.GetValue(entity) ?? 0);
            if (incomingId <= 0 || list.Any(e => (long)(idProp?.GetValue(e) ?? 0) == incomingId))
            {
                idProp?.SetValue(entity, id);
            }

            var createdProp = typeof(T).GetProperty("CreatedAt");
            createdProp?.SetValue(entity, DateTime.UtcNow);

            list.Add(entity);
            WriteToExcel(list);
            return entity;
        }

        public bool Delete(long id)
        {
            var list = GetAll();
            var idProp = typeof(T).GetProperty("ID");
            var removed = list.RemoveAll(e => (long)(idProp?.GetValue(e) ?? 0) == id) > 0;
            if (removed) WriteToExcel(list);
            return removed;
        }

        public T? GetByID(long id)
        {
            var idProp = typeof(T).GetProperty("ID");
            return GetAll().FirstOrDefault(e => (long)(idProp?.GetValue(e) ?? 0) == id);
        }

        public List<T> GetManyByFilter(Expression<Func<T, bool>> predicate) => GetAll().AsQueryable().Where(predicate).ToList();

        public T? GetOneByFilter(Expression<Func<T, bool>> predicate) => GetAll().AsQueryable().FirstOrDefault(predicate);

        public T Update(T entity)
        {
            var list = GetAll();
            var idProp = typeof(T).GetProperty("ID");
            var id = (long)(idProp?.GetValue(entity) ?? 0);
            var index = list.FindIndex(e => (long)(idProp?.GetValue(e) ?? 0) == id);
            if (index >= 0)
            {
                var updatedProp = typeof(T).GetProperty("UpdatedAt");
                updatedProp?.SetValue(entity, DateTime.UtcNow);
                list[index] = entity;
                WriteToExcel(list);
            }
            return entity;
        }
    }
}
