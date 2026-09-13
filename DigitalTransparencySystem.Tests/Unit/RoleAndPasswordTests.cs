using DigitalTransparencySystem.Helpers;
using NUnit.Framework;

namespace DigitalTransparencySystem.Tests.Unit
{
    [TestFixture]
    [Category("Unit")]
    public class RoleAccessTests
    {
        [Test(Description = "UT-01 RoleAccess.IsAdmin is true only for Admin")]
        public void UT01_AdminRoleIsRecognised()
        {
            Assert.That(RoleAccess.IsAdmin("Admin"), Is.True);
            Assert.That(RoleAccess.IsAdmin("Faculty"), Is.False);
            Assert.That(RoleAccess.IsAdmin("Student"), Is.False);
        }

        [Test(Description = "UT-02 Only Faculty and Staff may be event lead")]
        public void UT02_OnlyFacultyAndStaffMayBeEventLead()
        {
            Assert.That(RoleAccess.CanHoldEventAdmin("Faculty"), Is.True);
            Assert.That(RoleAccess.CanHoldEventAdmin("Staff"), Is.True);
            Assert.That(RoleAccess.CanHoldEventAdmin("Student"), Is.False);
            Assert.That(RoleAccess.CanHoldEventAdmin("Admin"), Is.False);
        }

        [Test(Description = "UT-03 Only Faculty may create assignments")]
        public void UT03_OnlyFacultyMayCreateAssignments()
        {
            Assert.That(RoleAccess.CanCreateAssignments("Faculty"), Is.True);
            Assert.That(RoleAccess.CanCreateAssignments("Staff"), Is.False);
            Assert.That(RoleAccess.CanCreateAssignments("Student"), Is.False);
        }

        [Test(Description = "UT-04 Admin cannot create events")]
        public void UT04_AdminIsNotACreateEventRole()
        {
            Assert.That(RoleAccess.CanCreateEvents("Admin"), Is.False);
            Assert.That(RoleAccess.CanCreateEvents("Faculty"), Is.True);
            Assert.That(RoleAccess.CanCreateEvents("Staff"), Is.True);
        }
    }

    [TestFixture]
    [Category("Unit")]
    public class PasswordHasherTests
    {
        [Test(Description = "UT-05 PasswordHasher hashes with PBKDF2 and verifies")]
        public void UT05_PasswordHasherHashAndVerify()
        {
            string hash = PasswordHasher.Hash("Campus#Test1");
            Assert.That(hash, Does.StartWith("pbkdf2$"));
            Assert.That(hash, Is.Not.EqualTo("Campus#Test1"));
            Assert.That(PasswordHasher.Verify("Campus#Test1", hash), Is.True);
            Assert.That(PasswordHasher.Verify("wrong-password", hash), Is.False);
        }
    }
}
