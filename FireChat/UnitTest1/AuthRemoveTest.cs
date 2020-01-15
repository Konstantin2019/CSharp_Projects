using FireBase_lib.Entities;
using FireBase_lib.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessangerActionsTests
{
    [TestClass]
    public class AuthRemoveTest
    {
        private static User CurrentUser { get; set; }
        private static MessangerActions fireBaseProvider = new MessangerActions();

        [TestMethod]
        public void AuthTestMethod()
        {
            #region Arrange
            User user_1 = new User { Name = "Kos", Value = "555" };
            CurrentUser = user_1;
            User user_2 = new User { Name = "Rolo", Value = "888" };
            User user_3 = new User { Name = "Kos", Value = "555" };
            var auth_expected_1 = 0;
            var auth_expected_2 = 1;
            var auth_expected_3 = -2;
            #endregion

            #region Act
            var auth_actual_1 = fireBaseProvider.AuthAsync(user_1).Result;
            var auth_actual_2 = fireBaseProvider.AuthAsync(user_2).Result;
            var auth_actual_3 = fireBaseProvider.AuthAsync(user_3).Result;
            #endregion

            #region Assert
            Assert.AreEqual(auth_expected_1, auth_actual_1);
            Assert.AreEqual(auth_expected_2, auth_actual_2);
            Assert.AreEqual(auth_expected_3, auth_actual_3);
            #endregion
        }

        [TestMethod]
        public void RemoveTestMethod()
        {
            #region Arrange
            User user = CurrentUser;
            var delete_expected = true;
            #endregion

            #region Act
            var delete_actual = fireBaseProvider.RemoveCurrentUserAsync().Result;
            #endregion

            #region Assert
            Assert.AreEqual(delete_expected, delete_actual);
            #endregion
        }
    }
}
