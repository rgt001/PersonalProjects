using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DisplayColorAttribute : Attribute
    {
        private static Type knownColorType = typeof(KnownColor);

        public DisplayColorAttribute(int color, bool paint = true)
        {
            Paint = paint;
            Color = IntToColor(color);
        }

        public DisplayColorAttribute(string methodConditionalColor, bool paint = true)
        {
            Paint = paint;
            MethodConditionalColor = methodConditionalColor;
        }

        private static Color IntToColor(int colorValue)
        {
            if (Enum.IsDefined(knownColorType, colorValue))
            {
                return Color.FromKnownColor((KnownColor)colorValue);
            }
            else
            {
                return Color.FromArgb(colorValue);
            }
        }

        public bool Paint { get; set; }
        public Color Color { get; set; }
        public string MethodConditionalColor { get; set; }
    }

    public class DisplayColorHelper
    {
        private readonly Type instanceType;
        private readonly DataGridView dataGridView;
        private readonly Dictionary<string, DisplayColorAttribute> propertyCache = new Dictionary<string, DisplayColorAttribute>();
        private readonly Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();
        private bool colorized = true;

        private Action<int, int, Color> updateCellColorAction;

        public DisplayColorHelper(Type instanceType, DataGridView dataGridView, bool colorize = true)
        {
            this.instanceType = instanceType;
            this.dataGridView = dataGridView;

            updateCellColorAction = (rowIndex, columnIndex, color) =>
            {
                if (dataGridView.InvokeRequired)
                {
                    dataGridView.Invoke((Action)(() =>
                    {
                        dataGridView.Rows[rowIndex].Cells[columnIndex].Style.BackColor = color;
                    }));
                }
                else
                {
                    dataGridView.Rows[rowIndex].Cells[columnIndex].Style.BackColor = color;
                }
            };

            var properties = instanceType.GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<DisplayColorAttribute>();
                if (attr != null)
                {
                    propertyCache[property.Name] = attr;

                    if (!string.IsNullOrEmpty(attr.MethodConditionalColor))
                    {
                        string methodKey = attr.MethodConditionalColor;
                        var method = instanceType.GetMethod(attr.MethodConditionalColor, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            methodCache[methodKey] = method;
                        }
                    }
                }
            }

            dataGridView.CellValidated += DataGridView_CellValueChanged;
            dataGridView.Sorted += async (s, e) => await ColorizeAsync();
            dataGridView.DataSourceChanged += async (s, e) => await ColorizeAsync();
            dataGridView.RowsAdded += async (s, e) => await ColorizeAsync(e.RowIndex);

            var form = GetParentForm(dataGridView);
            if (form != null)
            {
                form.FormClosing += Form_FormClosing;
            }

            colorized = colorize;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridView.CellValidated -= DataGridView_CellValueChanged;
            dataGridView.Sorted -= async (s, ee) => await ColorizeAsync();
            dataGridView.DataSourceChanged -= async (s, ee) => await ColorizeAsync();
            dataGridView.RowsAdded -= async (s, ee) => await ColorizeAsync(ee.RowIndex);
            if (sender is Form form)
                form.FormClosing -= Form_FormClosing;
        }

        private Form GetParentForm(Control control)
        {
            Control parent = control;
            while (parent != null)
            {
                if (parent is Form form)
                {
                    return form;
                }
                parent = parent.Parent;
            }
            return null;
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!colorized) return;
            _ = ColorizeAsync(e.RowIndex, e.ColumnIndex);
        }

        public Color GetDisplayColor(object instance, string propertyName)
        {
            if (propertyCache.TryGetValue(propertyName, out DisplayColorAttribute property) && property != null && property.Paint)
            {
                if (property.Color != Color.Empty)
                {
                    return property.Color;
                }
                else if (!string.IsNullOrEmpty(property.MethodConditionalColor))
                {
                    string methodKey = property.MethodConditionalColor;
                    if (methodCache.TryGetValue(methodKey, out MethodInfo method))
                    {
                        try
                        {
                            return (Color)method.Invoke(instance, new object[] { propertyName });
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Method '{property.MethodConditionalColor}' not found in class '{instanceType.FullName}'.");
                    }
                }
            }
            return Color.Empty;
        }

        public void StopEvents()
        {
            colorized = false;
        }

        public void StartEvents()
        {
            colorized = true;
            _ = ColorizeAsync();
        }

        public async Task ColorizeAsync()
        {
            if (!colorized) return;

            try
            {
                int batchSize = 50;
                int delay = 10;
                var rowCount = dataGridView.Rows.Count;
                var columnCount = dataGridView.Columns.Count;

                for (int i = 0; i < rowCount; i += batchSize)
                {
                    for (int j = 0; j < batchSize && (i + j) < rowCount; j++)
                    {
                        var row = dataGridView.Rows[i + j];
                        for (int k = 0; k < columnCount; k++)
                        {
                            try
                            {
                                var color = GetDisplayColor(row.DataBoundItem, dataGridView.Columns[k].DataPropertyName);
                                updateCellColorAction(i + j, k, color);
                            }
                            catch (Exception) { }
                        }
                    }
                    await Task.Delay(delay);
                }
            }
            catch (Exception) { }
        }

        public async Task ColorizeAsync(int rowIndex, int columnIndex)
        {
            if (!colorized) return;

            try
            {
                var row = dataGridView.Rows[rowIndex];
                var column = dataGridView.Columns[columnIndex];
                var color = GetDisplayColor(row.DataBoundItem, column.DataPropertyName);
                updateCellColorAction(rowIndex, columnIndex, color);
            }
            catch (Exception) { }
        }

        public async Task ColorizeAsync(int rowIndex)
        {
            if (!colorized) return;

            try
            {
                var row = dataGridView.Rows[rowIndex];
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    try
                    {
                        var color = GetDisplayColor(row.DataBoundItem, column.DataPropertyName);
                        updateCellColorAction(rowIndex, column.Index, color);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }
    }

}