using System.Reflection;

namespace apparelPro.BusinessLogic.Misc
{
    public class InputValidator
    {
        public static FilterResult Validate(string filterColumn, string filterQuery, Type type)
        {
            double n;
            DateTime res;
            FilterResult result = new();
           
            Dictionary<string,Type> fields = new Dictionary<string,Type>();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty) ;

            foreach (MethodInfo method in methods)
            {             
                if(method.Name.Substring(0,3) == "get")
                {
                    if(!fields.ContainsKey(method.Name))
                        fields.Add(method.Name.Substring(4), method.ReturnType);
                }
            }
           
            result.FilterQuery = filterQuery;
            result.FilterColumn = filterColumn;

            if (DateTime.TryParse(filterQuery, out res))
            {              
                var entry = new KeyValuePair<string, Type>();                
                foreach(var val in fields)
                {
                    switch(val.Value.ToString())
                    {
                        case "System.DateTime":
                            entry = val;
                            if (entry.Key.ToLower() == filterColumn.ToLower())
                            {
                                filterColumn = entry.Key.Substring(0, 1).ToLower()+entry.Key.Substring(1);
                            }
                            break;                       
                    }                   
                }
                var dt = Convert.ToDateTime(filterQuery);
                result.FilterColumn = filterColumn;
                result.FilterQuery = dt.ToString("MM/dd/yyyy");
                result.searchPattern = "{0} == (@0)";                
            }
            else if (double.TryParse(filterQuery, out n))
            {
                var entry = new KeyValuePair<string, Type>();
                foreach (var val in fields)
                {
                    switch (val.Value.ToString())
                    {
                        case "System.Nullable`1[System.Decimal]":
                            entry = val;
                            filterColumn = entry.Key.Substring(0, 1).ToLower() + entry.Key.Substring(1);                          
                            break;                            
                    }                  
                }
                result.FilterColumn = filterColumn;
                result.FilterQuery = filterQuery;
                result.searchPattern = "{0} >= (@0)";                             
            }
            else
            {
                var entry = new KeyValuePair<string, Type>();
                foreach (var val in fields)
                {
                    switch (val.Value.ToString())
                    {
                        case "System.String":
                            entry = val;
                            //filterColumn = entry.Key.Substring(0, 1).ToLower()+entry.Key.Substring(1);                           
                            break;
                    }
                
                    result.FilterColumn = filterColumn;
                    result.FilterQuery = filterQuery;
                  //  result.searchPattern = "{0} .StartsWith(@0)";
                    result.searchPattern = "{0} .Contains(@0)";
                }              
            }
            return result;
        }
    }

    public class FilterResult
    {
        public string? FilterColumn { get; set; }
        public string? FilterQuery { get; set; }
        public string? searchPattern { get; set; }
    }
}
