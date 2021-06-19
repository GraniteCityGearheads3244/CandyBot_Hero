using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Text;
using System.Threading;
using Math = System.Math;

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
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myRightCandy = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(4);
        static CTRE.Phoenix.MotorControl.CAN.VictorSPX myAgitator = new CTRE.Phoenix.MotorControl.CAN.VictorSPX(1);

        static OutputPort myFans = new OutputPort(CTRE.HERO.IO.Port5.Pin3, false);
        static OutputPort myLeftLED = new OutputPort(CTRE.HERO.IO.Port5.Pin4, false);
        static OutputPort myRightLED = new OutputPort(CTRE.HERO.IO.Port5.Pin5, false);
        static OutputPort myspare = new OutputPort(CTRE.HERO.IO.Port5.Pin6, false);
        static OutputPort myStackLight = new OutputPort(CTRE.HERO.IO.Port5.Pin7, false);



        static StringBuilder stringBuilder = new StringBuilder();

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        static double ConveyorSpeed = 0.6;
        static double CandySpeed = 0.5;
        static double AgitatorSpeed = .5;
        static float driveDirection = -1;

        public static void Main()
        {
      
           
            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                DriveDirectionFlip();
                Drive();
                Agiitator();
                LightAndFans();
                Candy();
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


        private static void Candy()
        {
            //Make sure there is a Joystick
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            if (_gamepad.GetButton(5))
            {
                if (driveDirection < 0)
                {
                    CandyBlue();
                }
                else
                {
                    CandyRed();
                }
               
            }
            else if (_gamepad.GetButton(6))
            {
                if (driveDirection < 0)
                {
                    CandyRed();
                }
                else
                {
                    CandyBlue();
                }

            }

            else
            {
                //Off
                myLeftLED.Write(false);
                myRightLED.Write(false);
                myLeftCandy.Set(ControlMode.PercentOutput, 0.0);
                myRightCandy.Set(ControlMode.PercentOutput, 0.0);
                myConveyor.Set(ControlMode.PercentOutput, 0.0);
            }
        }

        private static void CandyBlue()
        {
            //Left Candy
            myLeftLED.Write(true);
            myRightLED.Write(false);
            myLeftCandy.Set(ControlMode.PercentOutput, -CandySpeed);
            myRightCandy.Set(ControlMode.PercentOutput, 0.0);
            myConveyor.Set(ControlMode.PercentOutput, -ConveyorSpeed);

        }

        private static void CandyRed()
        {
            //Right Candy
            myLeftLED.Write(false);
            myRightLED.Write(true);
            myLeftCandy.Set(ControlMode.PercentOutput, 0.0);
            myRightCandy.Set(ControlMode.PercentOutput, -CandySpeed);
            myConveyor.Set(ControlMode.PercentOutput, ConveyorSpeed);
        }

        static void Drive()
        {
            //Make sure there is a Joystick
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            //float x = _gamepad.GetAxis(0);
            float speed = _gamepad.GetAxis(1) * driveDirection;
            float twist = _gamepad.GetAxis(2);

            //Deadband(ref x);
            Deadband(ref speed);
            Deadband(ref twist);

            float leftThrot = speed + twist;
            float rightThrot = speed - twist;

            leftDrive.Set(leftThrot);
            rightDrive.Set(-rightThrot);
         

            stringBuilder.Append("\t");
            stringBuilder.Append("driveDirection ");
            stringBuilder.Append(driveDirection);
            //stringBuilder.Append("\t");
            //stringBuilder.Append(y);
            //stringBuilder.Append("\t");
            //stringBuilder.Append(twist);

        }

        static void Agiitator()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            if (_gamepad.GetButton(1))
                myAgitator.Set(ControlMode.PercentOutput, AgitatorSpeed);
            else if (_gamepad.GetButton(2))
                myAgitator.Set(ControlMode.PercentOutput, -AgitatorSpeed);
            else
                myAgitator.Set(ControlMode.PercentOutput, 0);
        }

        static void LightAndFans()
        {
            
            myFans.Write(true);
            blinker();
            //myStackLight.Write(true);

     

        }

        static int scanCount = 0;
        private static void blinker()
        {
            if (scanCount < 25)
            {
                myStackLight.Write(true);
                scanCount++;
            }
            else
            {
                myStackLight.Write(false);
                scanCount++;
            }
            if (scanCount > 50)
            {
                scanCount = 0;
            }

        }

        static void DriveDirectionFlip()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            if (_gamepad.GetButton(10))
            { 
                driveDirection = -1;
            }
            else if (_gamepad.GetButton(9))
            {
                driveDirection = 1;
            }

        }
    }
}