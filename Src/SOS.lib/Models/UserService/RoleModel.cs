using System;

namespace SOS.Lib.Models.UserService
{
    public class RoleModel
    {
        public int Id { get; set; }

        public string Guid { get; set; }

        public string RoleName { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public int? AdministrationRoleId { get; set; }

        public int? UserAdministrationRoleId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ValidFromDate { get; set; }

        public DateTime? ValidToDate { get; set; }

        public int? OrganizationId { get; set; }

        public string Identifier { get; set; }

        public bool IsActivationRequired { get; set; }

        public int? MessageTypeId { get; set; }

        public bool IsUserAdministrationRole { get; set; }
    }
}
