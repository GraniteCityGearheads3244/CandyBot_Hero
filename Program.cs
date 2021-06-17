using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;
using System.Threading;

namespace _2021CandyBot
{
    public class Program
    {
        /* create DriveTrain PWM Motors */
        static PWMSpeedController leftDrive = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin6);
        static PWMSpeedController rightDrive = new PWMSpeedController(CTRE.HERO.IO.Port3.PWM_Pin4);

        /* create a talon, the Talon Device ID in HERO LifeBoat is zero */
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myConveyor = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(2);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myLeftCandy = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(3);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myRightCandy= new CTRE.Phoenix.MotorControl.CAN.VictorSPX(4);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myAgitator = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(1);

        static OutputPort myFans = new OutputPort(CTRE.HERO.IO.Port5.Pin3, false);
        static OutputPort myLeftLED = new OutputPort(CTRE.HERO.IO.Port5.Pin4, false);
        static OutputPort myRightLED = new OutputPort(CTRE.HERO.IO.Port5.Pin5, false);
        static OutputPort myspare = new OutputPort(CTRE.HERO.IO.Port5.Pin6, false);



        static StringBuilder stringBuilder = new StringBuilder();

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                Drive();
                Agiitator();
                LightAndFans();
                /* print whatever is in our string builder */
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();
                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();
                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref float value)
        {
            if (value < -0.10)
            {
                /* outside of deadband */
            }
            else if (value > +0.10)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }
        static void Drive()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            //float x = _gamepad.GetAxis(0);
            float y = -1 * _gamepad.GetAxis(2);
            float twist = _gamepad.GetAxis(1);

            //Deadband(ref x);
            Deadband(ref y);
            Deadband(ref twist);

            float leftThrot = y + twist;
            float rightThrot = y - twist;

            leftDrive.Set(leftThrot);
            rightDrive.Set(-rightThrot);
         

            stringBuilder.Append("\t");
            //stringBuilder.Append(x);
            stringBuilder.Append("\t");
            stringBuilder.Append(y);
            stringBuilder.Append("\t");
            stringBuilder.Append(twist);

        }

        static void Agiitator()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            if (_gamepad.GetButton(1))
                myAgitator.Set(ControlMode.PercentOutput, .2);
            else if (_gamepad.GetButton(2))
                myAgitator.Set(ControlMode.PercentOutput, -.2);
            else
                myAgitator.Set(ControlMode.PercentOutput, 0);
        }

        static void LightAndFans()
        {
            myLeftLED.Write(true);
            myFans.Write(true);
            myRightLED.Write(true);

        }
    }
}