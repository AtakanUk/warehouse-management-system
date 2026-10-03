using Warehouse.Core.Domain;
using Xunit;

namespace Warehouse.Core.Tests.Domain
{
    public class UserTests
    {
        [Theory]
        [InlineData("ab")]
        [InlineData("john doe")]
        [InlineData("<script>")]
        public void Rejects_invalid_user_names(string userName)
        {
            Assert.Throws<BusinessRuleException>(() => User.Create(userName, "hash", TestData.Now));
        }

        [Fact]
        public void Locks_out_after_repeated_failures_and_unlocks_after_the_lockout_period()
        {
            var user = User.Create("operator", "hash", TestData.Now);

            for (var attempt = 1; attempt < User.MaxFailedSignIns; attempt++)
            {
                user.RecordFailedSignIn(TestData.Now);
                Assert.False(user.IsLockedOut(TestData.Now));
            }

            user.RecordFailedSignIn(TestData.Now);

            Assert.True(user.IsLockedOut(TestData.Now));
            Assert.True(user.IsLockedOut(TestData.Now.Add(User.LockoutDuration).AddSeconds(-1)));
            Assert.False(user.IsLockedOut(TestData.Now.Add(User.LockoutDuration)));
        }

        [Fact]
        public void Successful_sign_in_resets_the_failure_counter()
        {
            var user = User.Create("operator", "hash", TestData.Now);
            user.RecordFailedSignIn(TestData.Now);
            user.RecordFailedSignIn(TestData.Now);

            user.RecordSuccessfulSignIn(TestData.Now);

            Assert.Equal(0, user.FailedSignInCount);
            Assert.Equal(TestData.Now, user.LastSignInAtUtc);
        }
    }
}
