using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Illumine.LPR.Repository
{
    public class CSVExecutor : BaseExecutor
    {
        public string FilePath { get; }

        public string Separator { get; }

        public CSVExecutor(string path, string separator)
          : base(path)
        {
            this.FilePath = path;
            this.Separator = separator;
        }

        public override void Create()
        {
            using (new StreamWriter(this.FilePath, false, Container.Get<Encoding>()))
            {

            }
        }

        public override void CreatePages(List<PageBase> pages)
        {
            using (StreamWriter sw = new StreamWriter(this.FilePath, false, Container.Get<Encoding>()))
                pages.ForEach(x =>
                {
                    sw.WriteLine("[" + x.PageName + "]");
                    sw.WriteLine(string.Join(this.Separator, ((IEnumerable<string>)x.FieldNames).Select(y => "\"" + y + "\"")));
                    sw.WriteLine("[/" + x.PageName + "]");
                });
        }

        private IEnumerable<string> ReadLines()
        {
            List<string> res = new List<string>();
            using (StreamReader sr = new StreamReader(this.FilePath, Container.Get<Encoding>()))
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                    res.Add(str);
            }
            return res;
        }

        private void WriteLines(IEnumerable<string> lines)
        {
            using (StreamWriter sw = new StreamWriter(FilePath, false, Container.Get<Encoding>()))
                lines.ToList().ForEach(line => sw.WriteLine(line));
        }

        public override void Delete<Data>(PageBase page, Data data)
        {
            List<string> list = ReadLines().ToList();
            int startIndex = list.IndexOf("[" + page.PageName + "]");
            int num = list.IndexOf("[/" + page.PageName + "]");
            int index = list.FindIndex(startIndex, num - startIndex, x => x.StartsWith(string.Format("\"{0}\"", data.Id)));
            list.RemoveAt(index);
            WriteLines(list);
        }

        public override void Insert<Data>(PageBase page, Data data)
        {
            List<string> list = ReadLines().ToList();
            int index = list.IndexOf("[/" + page.PageName + "]");
            Dictionary<string, string> dict = ItemConverter<Data>.GetDict(data);
            string str = string.Join(Separator, ((IEnumerable<string>)page.FieldNames).ToList().Select(x => "\"" + dict[x] + "\""));
            list.Insert(index, str);
            WriteLines(list);
        }

        public override void Update<Data>(PageBase page, Data data)
        {
            List<string> list = ReadLines().ToList();
            int startIndex = list.IndexOf("[" + page.PageName + "]");
            int num = list.IndexOf("[/" + page.PageName + "]");
            int index = list.FindIndex(startIndex, num - startIndex, x => x.StartsWith(string.Format("\"{0}\"", data.Id)));
            Dictionary<string, string> dict = ItemConverter<Data>.GetDict(data);
            string str = string.Join(Separator, ((IEnumerable<string>)page.FieldNames).ToList().Select(x => "\"" + dict[x] + "\""));
            list[index] = str;
            WriteLines(list);
        }

        public override List<Data> Read<Data>(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified)
        {
            List<string> list1 = this.ReadLines().ToList();
            int num1 = list1.IndexOf("[" + page.PageName + "]");
            int num2 = list1.IndexOf("[/" + page.PageName + "]");
            string title = list1[num1 + 1];
            List<Data> list2 = list1.GetRange(num1 + 2, num2 - num1 - 2).Select(l => ItemConverter<Data>.GetData(title, l, Separator[0])).ToList<Data>();
            switch (order)
            {
                case SortOrder.Ascending:
                    list2.Sort((x, y) => x.Id <= y.Id ? -1 : 1);
                    break;
                case SortOrder.Descending:
                    list2.Sort((x, y) => x.Id <= y.Id ? 1 : -1);
                    break;
            }
            return list2;
        }

        public override void ResetId<Data>(PageBase page, Data data, int newId)
        {
            List<string> list = ReadLines().ToList<string>();
            int startIndex = list.IndexOf("[" + page.PageName + "]");
            int num = list.IndexOf("[/" + page.PageName + "]");
            int index = list.FindIndex(startIndex, num - startIndex, x => x.StartsWith(string.Format("\"{0}\"", data.Id)));
            Dictionary<string, string> dict = ItemConverter<Data>.GetDict(data);
            dict["Id"] = newId.ToString();
            list[index] = string.Join(Separator, dict.Values.Select(x => "\"" + x + "\""));
            WriteLines(list);
        }

        public override bool TryGetFieldNames(PageBase page, out string[] FieldNames)
        {
            FieldNames = new string[] { };
            try
            {
                using (StreamReader sr = new StreamReader(this.FilePath, Container.Get<Encoding>()))
                {
                    while (!sr.EndOfStream)
                    {
                        if (sr.ReadLine() != "[" + page.PageName + "]")
                            continue;
                        FieldNames = sr.ReadLine().Split(new string[] { this.Separator }, StringSplitOptions.None);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
