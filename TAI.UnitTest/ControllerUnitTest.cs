using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TAI.Manager.VIS;
using TAI.Manager;
using TAI.Device;
using TAI.Constants;

namespace TAI.UnitTest
{
    [TestClass]
    public class ControllerUnitTest
    {
        [TestMethod]
        public void TestMethodVISController()
        {
            VISController controller = new VISController();
            controller.Initialize();
           /* controller.Open();*/
            OCRecognizeCommand command = new OCRecognizeCommand(controller,OCRType.ModelType);
         /*   string code = "";
            controller.QRModelSerialCode(ref code);

            Assert.AreEqual("24CHDigitalInput", controller.ModelType);*/

            command.ParseResponse("");
            Assert.AreEqual("AI", controller.ModelType);

            command.ParseResponse("{\"module\":1,\"type\":\"\"}");
            Assert.AreEqual("", controller.ModelType);

            command = new OCRecognizeCommand(controller, OCRType.ModelSerialCode);

            command.ParseResponse("{\"code\":\"KC94ES6010\"}");
            Assert.AreEqual("KC94ES6010", controller.ModelSerialCode);


/*            command = new OCRecognizeCommand(controller, OCRType.ChannelLighting);

            command.ParseResponse("{\"channels\": [{\"CH1\": 1}, {\"CH2\": 0}, {\"CH3\": 0}]}");
            Assert.AreEqual(3, controller.ChannelResults.Count);
            Assert.AreEqual("CH1", controller.ChannelResults[0].Key);*/





        }
    }
}
