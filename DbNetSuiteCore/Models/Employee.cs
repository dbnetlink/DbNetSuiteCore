using System;

namespace DbNetSuiteCore.Models
{
    public class Employee
    {
        public int EmployeeKey { get; set; }
        public int ParentEmployeeKey { get; set; }
        public string EmployeeNationalIDAlternateKey { get; set; } = string.Empty;
        public int SalesTerritoryKey { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public bool NameStyle { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public string LoginID { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public bool SalariedFlag { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int PayFrequency { get; set; }
        public double BaseRate { get; set; }
        public int VacationHours { get; set; }
        public int SickLeaveHours { get; set; }
        public bool CurrentFlag { get; set; }
        public bool SalesPersonFlag { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
    };
}
