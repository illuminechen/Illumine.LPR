using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR.Repository
{
  public abstract class BaseExecutor
  {
    public string FileName { get; }

    public BaseExecutor(string fileName) => this.FileName = fileName;

    public abstract void Create();

    public abstract bool TryGetFieldNames(PageBase page, out string[] FieldNames);

    public abstract void CreatePages(List<PageBase> pages);

    public virtual List<object> Read(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified)
    {
      MethodInfo methodInfo = ((IEnumerable<MethodInfo>) typeof (BaseExecutor).GetMethods()).First(x => x.IsGenericMethod && x.Name == nameof(Read)).MakeGenericMethod(page.DataType);
      IList instance = (IList) Activator.CreateInstance(typeof (List<>).MakeGenericType(page.DataType), methodInfo.Invoke(this, new object[3]
      {
         page,
         limit,
         order
      }));
      List<object> objectList = new List<object>();
      foreach (object obj in instance)
        objectList.Add(obj);
      return objectList;
    }

    public virtual void Insert(PageBase page, object data) => this.DoGenericMethod(nameof (Insert), page.DataType, page, data);

    public virtual void Update(PageBase page, object data) => this.DoGenericMethod(nameof (Update), page.DataType, page, data);

    public virtual void Delete(PageBase page, object data) => this.DoGenericMethod("Read", page.DataType, page, data);

    private void DoGenericMethod(string methodName, Type genericType, params object[] param) => ((IEnumerable<MethodInfo>) typeof (BaseExecutor).GetMethods()).First(x => x.IsGenericMethod && x.Name == methodName).MakeGenericMethod(genericType).Invoke(this, param);

    public abstract void Insert<Data>(PageBase page, Data data) where Data : class, IIndexData, new();

    public abstract void Update<Data>(PageBase page, Data data) where Data : class, IIndexData, new();

    public abstract List<Data> Read<Data>(PageBase page, int limit = -1, SortOrder order = SortOrder.Unspecified) where Data : class, IIndexData, new();

    public abstract void Delete<Data>(PageBase page, Data data) where Data : class, IIndexData, new();

    public abstract void ResetId<Data>(PageBase page, Data data, int newId) where Data : class, IIndexData, new();
  }
}
