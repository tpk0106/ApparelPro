using Microsoft.Extensions.Configuration;

namespace apparelPro.BusinessLogic.Services.Implementation.Registration
{
    public class PolicyManager
    {
        private readonly IConfiguration _configuration;
        private IEnumerable<IConfigurationSection> SubSections { get; set; }
        public PolicyManager(IConfiguration configuration)
        {
            _configuration = configuration;
            SubSections = configuration.GetSection("PolicyManager").GetChildren();           
        }

        public List<Section> Sections { get; set; } = [];

        public PolicyManager Load()
        {
            foreach (var section in SubSections)
            {
                var _section = new Section { Name = section.Key };                
                _section.Roles = [];

                foreach (var child in section.GetChildren())
                {
                    var roles = child.GetChildren()?.FirstOrDefault()?.Value?.Split(',').ToList();
                    foreach (var role in roles!)
                    {
                        var _role = new Role
                        {
                            Name = role
                        };
                        _section!.Roles.Add(_role);
                    }
                }
                Sections.Add(_section);
            }
            return this;
        }
    }

    public class Role
    {
        public string? Name { get; set; }
    }

    public class Section
    {
        public string? Name { get; set; }
        public List<Role>? Roles { get; set; }
    }

    public class Admin
    {
        public List<string> Roles { get; set; }
    }

    public class Merchandising
    {
        public List<string> Roles { get; set; }
    }
    public class PolicyManager1
    {
        public Merchandising Merchandising { get; set; }
        public Reports Reports { get; set; }
        public Stores Stores { get; set; }
        public RegisteredUser RegisteredUser { get; set; }
        public Admin Admin { get; set; }
    }

    public class RegisteredUser
    {
        public List<string> Roles { get; set; }
    }

    public class Reports
    {
        public List<string> Roles { get; set; }
    }

    public class Root
    {
        public PolicyManager PolicyManager { get; set; }
    }
    public class Stores
    {
        public List<string> Roles { get; set; }
    }
}
