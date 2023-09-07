using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utils.TreeViewExtension
{
    public static class TreeViewCreator
    {
        public static void PopulateTreeView<T>(this TreeView treeView, IEnumerable<T> values, string idProperty, string valueProperty, string fatherProperty, Func<T, bool> firstNode)
        {
            var categoryType = typeof(T);
            PropertyInfo idGet = categoryType.GetProperty(idProperty);
            PropertyInfo valueGet = categoryType.GetProperty(valueProperty);
            PropertyInfo fatherIdGet = categoryType.GetProperty(fatherProperty);

            treeView.CreateTreeView<T>(values.ToList(), idGet, valueGet, fatherIdGet, firstNode);
        }

        public static void CreateTreeView<T>(this TreeView treeView, List<T> values, PropertyInfo idGet, PropertyInfo valueGet, PropertyInfo fatherIdGet, Func<T, bool> firstNode)
        {
            foreach (T node in values.Where(firstNode))
            {
                var currentId = idGet.GetValue(node).ToString();
                var treeNode = treeView.Nodes.Add(currentId, valueGet.GetValue(node).ToString());
                PopulateNodes(treeNode, values, idGet, valueGet, fatherIdGet, p => fatherIdGet.GetValue(p).SafeToString() == currentId);
            }
        }

        private static void PopulateNodes<T>(TreeNode parentNode, List<T> values, PropertyInfo idGet, PropertyInfo valueGet, PropertyInfo fatherIdGet, Func<T, bool> where)
        {
            foreach (T node in values.Where(where))
            {
                var currentId = idGet.GetValue(node).ToString();
                var treeNode = parentNode.Nodes.Add(idGet.GetValue(node).ToString(), valueGet.GetValue(node).ToString());
                PopulateNodes(treeNode, values, idGet, valueGet, fatherIdGet, p => fatherIdGet.GetValue(p).SafeToString() == currentId);
            }
        }
    }
}
