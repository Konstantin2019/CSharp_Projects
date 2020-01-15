using FireChat.ViewModel.WPFServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessangerActionsTests
{
    [TestClass]
    public class WPFDialogTest
    {
        [TestMethod]
        public void WPFDialogTestMethod()
        {
            #region Arrange
            var dialog = new WPFDialogService();
            var text_info = "Информационное сообщение";
            var caption_info = text_info;
            var text_error = "Ошибка";
            var caption_error = text_error;
            var dialogResult_expected = 1;
            #endregion

            #region Act
            var dialogResult_actual_info = (int)dialog.ShowInfo(text_info, caption_info);
            var dialogResult_actual_error = (int)dialog.ShowError(text_error, caption_error);
            #endregion

            #region Assert
            Assert.AreEqual(dialogResult_expected, dialogResult_actual_info);
            Assert.AreEqual(dialogResult_expected, dialogResult_actual_error);
            #endregion
        }
    }
}
