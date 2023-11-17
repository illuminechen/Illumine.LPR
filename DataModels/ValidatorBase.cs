using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Illumine.LPR
{
    public abstract class ValidatorBase : IDataErrorInfo
    {
        public string Error => throw new NotSupportedException("IDataErrorInfo.Error is not supported, use IDataErrorInfo.this[propertyName] instead.");

        public string this[string propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("Invalid property name", propertyName);
                string str = string.Empty;
                object obj = this.GetValue(propertyName);
                List<ValidationResult> source = new List<ValidationResult>(1);
                ValidationContext validationContext = new ValidationContext((object)this, (IServiceProvider)null, (IDictionary<object, object>)null);
                validationContext.MemberName = propertyName;
                List<ValidationResult> validationResultList = source;
                if (!Validator.TryValidateProperty(obj, validationContext, (ICollection<ValidationResult>)validationResultList))
                    str = source.First<ValidationResult>().ErrorMessage;

                if (obj != null && obj.ToString() != "" && GetType().GetProperty(propertyName).GetCustomAttributes().OfType<UniqueAttribute>().ToList().Count != 0)
                {

                    Type genericListType = typeof(ObservableCollection<>).MakeGenericType(GetType());
                    var list = (IList)Activator.CreateInstance(genericListType, new object[] { this.GetValue("list") });
                    if (list != null)
                    {
                        bool once = false;
                        foreach (var o in list)
                        {
                            var value = this.GetType().GetProperty(propertyName).GetValue(o);
                            if (value?.ToString() == obj.ToString())
                            {
                                if (once)
                                {
                                    str = "值重複";
                                    break;
                                }
                                once = true;
                            }
                        }
                    }
                }

                return str;
            }
        }
        private object GetValue(string propertyName) => this.GetType().GetProperty(propertyName).GetValue((object)this);
    }
}
