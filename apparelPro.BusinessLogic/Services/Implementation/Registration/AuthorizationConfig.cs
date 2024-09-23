using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class AuthorizationConfig
    {
        private static IConfiguration? _configuration;

        private static PolicyManager? _policyManager { get; set; }
        public void Inject(IConfiguration configuration)
        {
            _configuration = configuration;
            PolicyManager policyManager = new PolicyManager(configuration);
            _policyManager = policyManager!.Load();
        }

        public static void GetAuthroizationOptions(AuthorizationOptions options)
        {
            //AddAllPolicies(options, _policyManager!);
       
            AddMerchandisingPoliciesUsingPolicyManager(options, _policyManager!,"Merchandising", null);
            AddStorePoliciesUsingPolicyManager(options, _policyManager!,"Stores",null);
            AddProductionPoliciesUsingPolicyManager(options, _policyManager!,"Production",null);
            AddOfficePoliciesUsingPolicyManager(options, _policyManager!,"Office",null);
            AddImportExportPoliciesUsingPolicyManager(options, _policyManager!,"ImportExport",null);
            AddRegisteredUserPoliciesUsingPolicyManager(options, _policyManager!,"RegisteredUser",null);

            //AddMerchandisingDepartmentpolicies(options);         
            //AddStoresPolicies(options);
            //AddProductionPolicies(options);
            //AddOfficePolicies(options);
            //AddImportExportPolicies(options);
        }    

        private static void AddRegisteredUserPoliciesUsingPolicyManager(
            AuthorizationOptions options, 
            PolicyManager policyManager, 
            string policyName, 
            string exclludes
        )
        {
            PermissionConfiguration.Configure(options, policyManager, policyName, null); ;
        }

        public static void AddStoresPolicies(AuthorizationOptions options)
        {
            StoreManager storeManager = new StoreManager();
            PermissionConfiguration.Configure(storeManager, options, "StoreManager",
                _configuration, "PolicyManager:Stores:Roles", null);
        }

        public static void AddMerchandisingDepartmentpolicies(AuthorizationOptions options)
        {
            MerchandisingManager? merchandsingManager = new MerchandisingManager();

            PermissionConfiguration.Configure(merchandsingManager, options, "Merchandising Manager",
                _configuration, "PolicyManager:Merchandising:Roles", null);
            PermissionConfiguration.Configure(merchandsingManager, options, "Merchandiser",
                _configuration, "PolicyManager:Merchandising:Roles", "Order Entry Operator");
            PermissionConfiguration.Configure(merchandsingManager, options, "Developer",
               _configuration, "PolicyManager:Merchandising:Roles", null);
        }      

        public static void AddMerchandisingPoliciesUsingPolicyManager(AuthorizationOptions options, PolicyManager policyManager, string policyName, string excludes)
        {
            PermissionConfiguration.Configure(options, policyManager,policyName, excludes);
        }

        private static void AddStorePoliciesUsingPolicyManager(AuthorizationOptions options, PolicyManager policyManager, string policyName, string excludes)
        {
            PermissionConfiguration.Configure(options, policyManager, policyName, excludes);
        }

        public static void AddProductionPoliciesUsingPolicyManager(AuthorizationOptions options, PolicyManager policyManager, string policyName, string excludes)
        {
            PermissionConfiguration.Configure(options, policyManager, policyName, excludes);
        }

        public static void AddOfficePoliciesUsingPolicyManager(AuthorizationOptions options, PolicyManager policyManager,string policyName, string excludes)
        {
            PermissionConfiguration.Configure(options, policyManager, policyName, null); ;
        }

        public static void AddImportExportPoliciesUsingPolicyManager(AuthorizationOptions options, PolicyManager policyManager,string policyName, string excludes)
        {
            PermissionConfiguration.Configure(options, policyManager, policyName, excludes);
        }

        private static void AddImportExportPolicies(AuthorizationOptions options)
        {
            // PermissionConfiguration.Configure(options, "MerchandisingManager", _configuration, "PolicyManager:Merchandising:Roles", null);
        }

        public static void AddOfficePolicies(AuthorizationOptions options)
        {
            //   PermissionConfiguration.Configure(options, "CEO", _configuration, "PolicyManager:Merchandising:Roles", null);
            //   PermissionConfiguration.Configure(options, "OfficeAdmin", _configuration, "PolicyManager:Merchandising:Roles", null);
        }

        private static void AddProductionPolicies(AuthorizationOptions options)
        {
            // PermissionConfiguration.Configure(options, "ProductionManager", _configuration, "PolicyManager:Merchandising:Roles", null);
        }
        public static void AddAllPolicies(AuthorizationOptions options, PolicyManager policyManager)
        {
            foreach (var section in policyManager.Sections)
            {
                foreach (var Role in section.Roles!)
                {
                    PermissionConfiguration.Configure(options, section.Name!, Role.Name!, null);
                }
            }
        }

        public class PermissionConfiguration
        {           

            public static void Configure(AuthorizationOptions options, PolicyManager policyManager,
               string policyName, string? excludes)
            {               
                List<string> excludesList = [];

                if (excludes != null && excludes.Length > 0)
                {
                    excludesList = excludes.Trim().Split(',').ToList();
                }

                foreach (var section in policyManager.Sections.Where(s => s.Name == policyName))
                {
                    foreach (var _role in section.Roles!.ToList().Where(r => !excludesList.Contains(r.Name!)))
                    {                      
                        options.AddPolicy(policyName, policy =>
                        {
                            if (_role.Name!.Trim().Length > 0) policy.RequireRole(_role.Name!);
                            policy.RequireAuthenticatedUser();
                        });
                    }
                }                
            }

            public static void Configure(AuthorizationOptions options,
               string policyName, string role, string? excludes)
            {
                options.AddPolicy(policyName, policy =>
                {
                    if (role.Trim().Length > 0) policy.RequireRole(role!);
                    policy.RequireAuthenticatedUser();
                });
            }

            public static void Configure(IRootConfiguration configManager, AuthorizationOptions options,
                string policyName, IConfiguration configuration, string key, string? excludes)
            {
                IRootConfiguration configurationManager = (IRootConfiguration)configManager;
                var section = configuration.GetSection(key);

                var list = section.GetChildren().ToList();
                var roles = list[0].Value?.Split(',').ToList();

                if (excludes != null)
                {
                    var rolesToExclude = excludes.Split(',').ToList();
                    var filtered = roles!.Where(r => !rolesToExclude.Contains(r)).ToList();
                    roles = filtered;
                }

                configurationManager.Roles = roles!;

                options.AddPolicy(policyName, policy =>
                {
                    foreach (var role in configurationManager.Roles!)
                    {
                        policy.RequireRole(role!);
                    }
                    policy.RequireAuthenticatedUser();
                });
            }
        }
    }
}
