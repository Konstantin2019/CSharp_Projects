using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VkBot;

namespace UnitTests
{
    [TestClass]
    public class TestVkService
    {
        [TestMethod]
        public void TestGetPrivateInfo()
        {
            #region Arrange
            var vkServiceToTest = new VkService();
            var ids_path = "test_info.txt";
            var client_id_expected = 9474548;
            var group_id_expected = 146932531;
            Type t = typeof(VkService);
            MethodInfo method = t.GetMethod("GetPrivateInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            #endregion

            #region Act
            var ids_actual = (Dictionary<string, int>)method.Invoke(vkServiceToTest, new object[] { ids_path });
            #endregion

            #region Assert
            Assert.AreEqual(client_id_expected, ids_actual["client_id"]);
            Assert.AreEqual(group_id_expected, ids_actual["group_id"]);
            #endregion
        }
    }
}
