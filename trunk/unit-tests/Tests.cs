using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

using LateBindingHelper;
using LateBindingHelper.Exceptions;


namespace LateBindingHelper.Tests
{
    [TestFixture]
    public class LateBindingHelperTestFixture
    {
        #region Test Members

        IOperationInvoker _lateBindingFacade;

        #endregion

        #region TestFixture SetUp/TearDown

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
        }

        #endregion

        #region  Test SetUp/TearDown

        [SetUp]
        public void SetUp()
        {
            _lateBindingFacade = BindingFactory.CreateObjectBinding(typeof(MyLateBindingTestType), 27);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        #region Tests
        [Test]
        public void TestFieldAccess()
        {
            Assert.That(_lateBindingFacade.Field("myField").Get<Int32>(), Is.EqualTo(27));

            _lateBindingFacade.Field("myField").Set(-50);

            Assert.That(_lateBindingFacade.Field("myField").Get<Int32>(), Is.EqualTo(-50));
        }

        [Test]
        public void TestSimpleMethodCall()
        {
            _lateBindingFacade.Method("SimpleMethod").Invoke();
        }

        [Test]
        [ExpectedException(typeof(OperationCallFailedException))]
        public void TestMethodCall_Failure()
        {
            _lateBindingFacade.Method("blerz").Invoke();
        }

        [Test]
        public void TestMethodCall()
        {
            int sum = _lateBindingFacade.Method("Sum")
                .AddParameter(15)
                .AddParameter(17)
                .Invoke<int>();

            Assert.That(sum, Is.EqualTo(15 + 17));
        }

        [Test]
        public void TestMethodWithReferenceValues()
        {
            int refValue = 10;
            object tmp = refValue;

            _lateBindingFacade.Method("MulFiveRef").AddRefParameter(tmp).Invoke();

            refValue = (int) _lateBindingFacade.LastCallParameters[0];

            Assert.That(refValue, Is.EqualTo(10 * 5));
        }

        [Test]
        public void TestMethodWithMultipleReferenceValues()
        {
            int refValue1 = 10;
            int refValue2 = 20;

            object[] args = new object[] { refValue1, refValue2 };

            _lateBindingFacade
                .Method("MulTenRef")
                .AddRefParameter(args[0])
                .AddRefParameter(args[1])
                .Invoke();

            refValue1 = (int)_lateBindingFacade.LastCallParameters[0];
            refValue2 = (int)_lateBindingFacade.LastCallParameters[1];

            Assert.That(refValue1, Is.EqualTo(10 * 10));
            Assert.That(refValue2, Is.EqualTo(20 * 10));
        }

        [Test]
        public void TestPropertiesAccess()
        {
            Assert.That(_lateBindingFacade.Property("MyProp").Get<int>(), Is.EqualTo(27));

            _lateBindingFacade.Property("MyProp").Set(69);
            Assert.That(_lateBindingFacade.Property("MyProp").Get<Int32>(), Is.EqualTo(69));
        }

        [Test, ExpectedException(typeof(OperationCallFailedException))]
        public void TestBadIndexerAccess()
        {
            _lateBindingFacade.Index(5).Get<string>();
        }

        [Test]
        public void TestIndexerAccess()
        {
            _lateBindingFacade.Index(5).Set("[myValue]");

            Assert.That(_lateBindingFacade.Index(5).Get<string>(), Is.EqualTo(("[myValue]")));
        }

        [Test,Ignore]
        public void WordAutomationTest_NotReallyATestUnit()
        {
            IOperationInvoker wordApp = BindingFactory.CreateAutomationBinding("Word.Application");

            //Get Word objects to interop operations
            IOperationInvoker document = wordApp.Property("Documents").Get();
            
            document
                .Method("Add")
                .Invoke();
            IOperationInvoker selection = wordApp.Property("Selection").Get();

            string str = "Hello World!";

            //Make Word visible
            wordApp.Property("Visible").Set(true);

            //Activate bold
            selection.Method("BoldRun").Invoke();

            //Change font size
            selection.Property("Font").Get().Property("Size").Set(20);

            foreach (char c in str)
            {
                selection.Method("TypeText").AddParameter(c.ToString()).Invoke();
                System.Threading.Thread.Sleep(200);
            }

            //We need to get the type of the enumeration, that breaks the Late Binding approach as we need
            //a way to know a specific type, and it's assembly
            //Type enumType = Assembly
            //      .LoadWithPartialName("Microsoft.Office.Interop.Word")
            //      .GetType("Microsoft.Office.Interop.Word.WdSaveOptions", false);
            //word.Call("Quit", Activator.CreateInstance(enumType)));

            //But if we know the equivalent int to the enum value it will work
            //WdSaveOptions.wdDoNotSaveChanges == 0

            //Hide Word
            bool visibility = wordApp.Property("Visible").Get<bool>();
            visibility = !visibility;
            wordApp.Property("Visible").Set(visibility);

            //Quit
            wordApp.Method("Quit").AddParameter(0).Invoke();
        }
        #endregion
    }
}