using System;

namespace DigitalTransparencySystem.Helpers
{
    public static class RoleAccess
    {
        public static bool IsAdmin(string role)
        {
            return !string.IsNullOrEmpty(role)
                && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsFacultyOrStaff(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            return role.Equals("Faculty", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Staff", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsFaculty(string role)
        {
            return !string.IsNullOrEmpty(role)
                && role.Equals("Faculty", StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanCreateEvents(string role)
        {
            return IsFacultyOrStaff(role) || IsStudent(role);
        }

        public static bool CanCreateClubs(string role)
        {
            return CanCreateEvents(role);
        }

        public static bool CanHoldEventAdmin(string role)
        {
            return IsFacultyOrStaff(role);
        }

        public static bool IsStudent(string role)
        {
            return !string.IsNullOrEmpty(role)
                && role.Equals("Student", StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanCreateAssignments(string role)
        {
            return IsFaculty(role);
        }

        public static bool CanMonitorAssignments(string role)
        {
            return CanCreateAssignments(role);
        }
    }
}
