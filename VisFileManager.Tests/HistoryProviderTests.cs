using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisFileManager.Tests
{
    [TestClass]
    public class HistoryProviderTests
    {
        [TestMethod]
        public void UndoShouldReturnPreviousItem()
        {
            HistoryProvider<string> provider = new HistoryProvider<string>();

            provider.Push("1");
            provider.Push("2");
            Assert.AreEqual("2", provider.Undo());
        }

        [TestMethod]
        public void RedoItemShouldBePreviousItem()
        {
            HistoryProvider<string> provider = new HistoryProvider<string>();

            provider.Push("1");
            provider.Push("2");
            provider.Undo();
            
            Assert.AreEqual("2", provider.RedoItem);
        }

        [TestMethod]
        public void RedoShouldBeDestroyedWhenNewItemAdded()
        {
            HistoryProvider<string> provider = new HistoryProvider<string>();

            provider.Push("1");
            provider.Push("2");
            provider.Undo();
            
            Assert.AreEqual("2", provider.RedoItem);

            provider.Push("3");

            Assert.AreEqual(false, provider.CanRedo);
        }

        [TestMethod]
        public void UndoNotAvailableWhenNoHistory()
        {
            HistoryProvider<string> provider = new HistoryProvider<string>();
            
            Assert.AreEqual(false, provider.CanUndo);
        }

        [TestMethod]
        public void RedoNotAvailableWhenNoHistory()
        {
            HistoryProvider<string> provider = new HistoryProvider<string>();

            Assert.AreEqual(false, provider.CanRedo);
        }
    }
}
