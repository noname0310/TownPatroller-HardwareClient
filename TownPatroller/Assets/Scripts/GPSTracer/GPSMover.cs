using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TownPatroller.CarDevice;
using TownPatroller.PhoneDevice.GPS;
using TPPacket.Class;

namespace TownPatroller.GPSTracer
{
    public class GPSMover : MonoBehaviour
    {
        public GPSSpotManager GPSSpotManager;
        private BaseCarDivice carDivice;
        public bool EnableTraceMode { get; set; }

        MoveSequence CurrentMoveSequence;
        ProgressiveLengthMeasureSubSequence ProgressiveLengthMeasureSubSequence;
        Direction TraceDraction;

        bool rotationSaved;
        float reqrotation;

        bool LBlocked;
        bool RBlocked;
        int CurrentMoveFrontCount;
        int MoveFrontTickCount;

        bool TurnBack;

        float BetweenAngle;

        public void _new(BaseCarDivice baseCarDivice)
        {
            GPSSpotManager = new GPSSpotManager(0);
            //GPSSpotManager.AddPos(new GPSsPosition(10, 10).GetGPSPosition());
            carDivice = baseCarDivice;
            EnableTraceMode = true;
            CurrentMoveSequence = MoveSequence.ForcedForward;

            carDivice.statusparser.OnParsedSOP += Statusparser_OnParsedSOP;
        }

        public void ChangeSpotManager(GPSSpotManager gPSSpotManager)
        {
            GPSSpotManager = gPSSpotManager;
        }

        public string GetCurrentPositonName()
        {
            if (GPSSpotManager.GPSPositions.Count <= 1)
                return "N/A";
            else
            {
                if (GPSSpotManager.CurrentMovePosIndex == 0)
                    return GPSSpotManager.GPSPositions.Last().LocationName;
                else
                    return GPSSpotManager.GPSPositions[GPSSpotManager.CurrentMovePosIndex - 1].LocationName;
            }
        }

        private void Statusparser_OnParsedSOP()
        {
            if (EnableTraceMode)
            {
                if (GPSSpotManager.GPSPositions.Count == 0)
                {
                    MoveFront(0);
                    return;
                }

                IGConsole.Instance.Main.println(CurrentMoveSequence.ToString());

                GPSsPosition MyPos = /*new GPSsPosition(0, 0);//*/GPSCore.Instance.GetGPSsPosition();
                GPSsPosition TargetPos = GPSSpotManager.GPSPositions[GPSSpotManager.CurrentMovePosIndex].GetGPSS();

                float x = Mathf.Abs(MyPos.longitude - TargetPos.longitude);
                float y = Mathf.Abs(MyPos.latitude - TargetPos.latitude);

                float Distance = Mathf.Sqrt(x * x + y * y);
                float ReqAngleFromN = CalcReqAngleFromN(MyPos, TargetPos);

                if (Distance < 0.00006)
                {
                    GPSSpotManager.MoveNext();
                    return;
                }

                switch (CurrentMoveSequence)
                {
                    case MoveSequence.ForcedForward:
                        #region 1-1
                        if (RotateToReqAngle(ReqAngleFromN) == false)
                        {
                            if (carDivice.rh_sonardist > 10 && carDivice.lh_sonardist > 10)
                            {
                                MoveBack(255);
                            }
                            else if (carDivice.rh_sonardist > 10)
                            {
                                RotationL(255);
                            }
                            else if (carDivice.lh_sonardist > 10)
                            {
                                RotationR(255);
                            }
                            else if (carDivice.f_sonardist < 30)
                            {
                                RBlocked = false;
                                LBlocked = false;
                                rotationSaved = false;
                                CurrentMoveSequence = MoveSequence.Rotat90To2n2;
                                goto case MoveSequence.Rotat90To2n2;
                            }
                            else if (carDivice.rs_sonardist < 20)
                            {
                                TraceDraction = Direction.Right;
                                CurrentMoveSequence = MoveSequence.SemiAutomaticForward;
                                goto case MoveSequence.SemiAutomaticForward;
                            }
                            else if (carDivice.ls_sonardist < 20)
                            {
                                TraceDraction = Direction.Left;
                                CurrentMoveSequence = MoveSequence.SemiAutomaticForward;
                                goto case MoveSequence.SemiAutomaticForward;
                            }
                            else
                            {
                                MoveFront(255);
                            }
                        }
                        break;
                    #endregion

                    case MoveSequence.SemiAutomaticForward:
                        #region 1-2
                        BetweenAngle = GetBetweenTargetAngle(ReqAngleFromN);

                        if (BetweenAngle > 60)
                        {
                            Direction direction = GetRotateTargetDerection(ReqAngleFromN);

                            if (direction == Direction.Right && TraceDraction == Direction.Left)
                            {
                                CurrentMoveSequence = MoveSequence.ForcedForward;
                                goto case MoveSequence.ForcedForward;
                            }
                            else if (direction == Direction.Left && TraceDraction == Direction.Right)
                            {
                                CurrentMoveSequence = MoveSequence.ForcedForward;
                                goto case MoveSequence.ForcedForward;
                            }

                            if (BetweenAngle > 80)
                            {
                                if (direction == Direction.Right && TraceDraction == Direction.Right)
                                {
                                    RBlocked = true;
                                    LBlocked = false;

                                    MoveFrontTickCount = 100;
                                    CurrentMoveFrontCount = 0;
                                    //TraceDraction = TraceDraction;

                                    CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                    ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                    goto case MoveSequence.ProgressiveLengthMeasure;
                                }
                                else if (direction == Direction.Left && TraceDraction == Direction.Left)
                                {
                                    RBlocked = false;
                                    LBlocked = true;

                                    MoveFrontTickCount = 100;
                                    CurrentMoveFrontCount = 0;
                                    //TraceDraction = TraceDraction;

                                    CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                    ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                    goto case MoveSequence.ProgressiveLengthMeasure;
                                }
                            }
                        }

                        else if (carDivice.f_sonardist < 30)//////////////////////////
                        {
                            if (TraceDraction == Direction.Right)
                            {
                                RBlocked = true;
                                LBlocked = false;
                            }
                            else
                            {
                                RBlocked = false;
                                LBlocked = true;
                            }
                            rotationSaved = false;
                            CurrentMoveSequence = MoveSequence.Rotat90To2n2;
                            goto case MoveSequence.Rotat90To2n2;
                        }
                        else
                        {
                            DriveWall(TraceDraction);
                        }
                        break;
                    #endregion

                    case MoveSequence.Rotat90To2n2:
                        #region 2-1
                        if (rotationSaved == false)//reqrotation 설정
                        {
                            rotationSaved = true;

                            if (RBlocked == true)
                            {
                                reqrotation = (CompassCore.Instance.AngleFromN - 90) % 360;
                                TraceDraction = Direction.Right;
                            }
                            else if(LBlocked == true)
                            {
                                reqrotation = (CompassCore.Instance.AngleFromN + 90) % 360;
                                TraceDraction = Direction.Left;
                            }
                            else
                            {
                                Direction direction = GetRotateTargetDerection(ReqAngleFromN);

                                if (direction == Direction.Right)
                                {
                                    reqrotation = (CompassCore.Instance.AngleFromN + 90) % 360;
                                    TraceDraction = Direction.Left;
                                }
                                else//왼쪽에 ReqAngleFromN존재
                                {
                                    reqrotation = (CompassCore.Instance.AngleFromN - 90) % 360;
                                    TraceDraction = Direction.Right;
                                }
                            }
                        }
                        if (RotateToReqAngle(reqrotation) == false)
                        {
                            //RBlocked = RBlocked;
                            //LBlocked = LBlocked;

                            MoveFrontTickCount = 100;
                            CurrentMoveFrontCount = 0;
                            //TraceDraction = TraceDraction;

                            CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                            ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                            goto case MoveSequence.ProgressiveLengthMeasure;
                        }
                        break;
                    #endregion

                    case MoveSequence.ProgressiveLengthMeasure:
                        #region 2-2
                        BetweenAngle = GetBetweenTargetAngle(ReqAngleFromN);

                        if (BetweenAngle > 130 && ProgressiveLengthMeasureSubSequence != ProgressiveLengthMeasureSubSequence.Rotate180)
                        {
                            //TraceDraction = TraceDraction;
                            TurnBack = false; 
                            CurrentMoveSequence = MoveSequence.RetractionMeasure;
                            goto case MoveSequence.RetractionMeasure;
                        }
                        else if (BetweenAngle < 70 && ProgressiveLengthMeasureSubSequence != ProgressiveLengthMeasureSubSequence.Rotate180)
                        {
                            //TraceDraction = TraceDraction;
                            CurrentMoveSequence = MoveSequence.SemiAutomaticForward;
                            goto case MoveSequence.SemiAutomaticForward;
                        }
                        else
                        {
                            switch (ProgressiveLengthMeasureSubSequence)
                            {
                                case ProgressiveLengthMeasureSubSequence.Front:
                                    if (carDivice.f_sonardist < 30)
                                    {
                                        if (TraceDraction == Direction.Right)
                                        {
                                            LBlocked = true;
                                            if (!RBlocked)
                                            {
                                                rotationSaved = false;
                                                ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Rotate180;
                                                goto case ProgressiveLengthMeasureSubSequence.Rotate180;
                                            }
                                        }
                                        else
                                        {
                                            RBlocked = true;
                                            if (!LBlocked)
                                            {
                                                rotationSaved = false;
                                                ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Rotate180;
                                                goto case ProgressiveLengthMeasureSubSequence.Rotate180;
                                            }
                                        }
                                        if (LBlocked && RBlocked)
                                        {
                                            rotationSaved = false;
                                            ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Rotate90;
                                            goto case ProgressiveLengthMeasureSubSequence.Rotate90;
                                        }
                                    }
                                    else if (MoveFrontTickCount == CurrentMoveFrontCount)
                                    {
                                        rotationSaved = false;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Rotate180;
                                        goto case ProgressiveLengthMeasureSubSequence.Rotate180;
                                    }
                                    else
                                    {
                                        CurrentMoveFrontCount++;
                                        DriveWall(TraceDraction);
                                    }
                                    break;

                                case ProgressiveLengthMeasureSubSequence.Rotate180:
                                    if (rotationSaved == false)
                                    {
                                        rotationSaved = true;
                                        reqrotation = (CompassCore.Instance.AngleFromN + 180) % 360;
                                    }

                                    if (RotateToReqAngle(reqrotation) == false)
                                    {
                                        MoveFrontTickCount *= MoveFrontTickCount;
                                        CurrentMoveFrontCount = 0;
                                        TraceDraction = (TraceDraction == Direction.Right) ? Direction.Left : Direction.Right;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                        goto case ProgressiveLengthMeasureSubSequence.Front;
                                    }
                                    break;

                                case ProgressiveLengthMeasureSubSequence.Rotate90:
                                    if (rotationSaved == false)
                                    {
                                        rotationSaved = true; 
                                        
                                        if (TraceDraction == Direction.Left)
                                        {
                                            reqrotation = (CompassCore.Instance.AngleFromN + 90) % 360;
                                        }
                                        else
                                        {
                                            reqrotation = (CompassCore.Instance.AngleFromN - 90) % 360;
                                        }
                                    }
                                    if (RotateToReqAngle(reqrotation) == false)
                                    {
                                        //XXXXXXXXgoto2-3
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;
                    #endregion

                    case MoveSequence.RetractionMeasure:
                        #region 2-3
                        if (TurnBack == false)
                        {
                            TurnBack = !RotateToReqAngle((ReqAngleFromN + 180) % 360);
                        }
                        else
                        {
                            if (carDivice.f_sonardist < 30)
                            {
                                CurrentMoveSequence = MoveSequence.ForcedForward;
                                goto case MoveSequence.ForcedForward;
                                //goto 2-4
                            }
                            else
                            {
                                BetweenAngle = GetBetweenTargetAngle(ReqAngleFromN);
                                Direction direction = GetRotateTargetDerection(ReqAngleFromN);

                                if (BetweenAngle < 120)
                                {
                                    if (TraceDraction == direction)//바깥쪽으로 나갈때
                                    {
                                        if (TraceDraction == Direction.Right)
                                        {
                                            RBlocked = true;
                                            LBlocked = false;
                                        }
                                        else
                                        {
                                            RBlocked = false;
                                            LBlocked = true;
                                        }

                                        MoveFrontTickCount = 100;
                                        CurrentMoveFrontCount = 0;
                                        //TraceDraction = TraceDraction;

                                        CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                        goto case MoveSequence.ProgressiveLengthMeasure;
                                    }
                                    else//안쪽으로 들어갈때
                                    {
                                        CurrentMoveSequence = MoveSequence.ForcedForward;
                                        goto case MoveSequence.ForcedForward;
                                        //goto 2-4
                                    }
                                }
                                DriveWall(TraceDraction);
                            }
                        }
                        break;
                    #endregion

                    case MoveSequence.Root:
                        #region 2-4

                        break;
                    #endregion

                    default:
                        break;
                }
            }
        }

        #region CalcMoveMethods

        private void DriveWall(Direction direction)
        {
            if (direction == Direction.Left)
            {
                if (45 < carDivice.ls_sonardist)
                    MoveFront(90, 255);
                else if (40 < carDivice.ls_sonardist)
                    MoveFront(100, 255);
                else if (35 < carDivice.ls_sonardist)
                    MoveFront(100, 255);
                else if (carDivice.ls_sonardist < 15)
                    MoveFront(255, 50);
                else if (carDivice.ls_sonardist < 20)
                    MoveFront(255, 70);
                else if (carDivice.ls_sonardist < 30)
                    MoveFront(255, 150);
                else
                    MoveFront(255);
            }
            else
            {
                if (45 < carDivice.rs_sonardist)
                    MoveFront(255, 90);
                else if (40 < carDivice.rs_sonardist)
                    MoveFront(255, 100);
                else if (35 < carDivice.rs_sonardist)
                    MoveFront(255, 150);
                else if (carDivice.rs_sonardist < 15)
                    MoveFront(50, 255);
                else if (carDivice.rs_sonardist < 20)
                    MoveFront(70, 255);
                else if (carDivice.rs_sonardist < 30)
                    MoveFront(150, 255);
                else
                    MoveFront(255);
            }
        }

        private bool RotateToReqAngle(float ReqAngleFromN)
        {
            float BtweenAngle = GetBetweenTargetAngle(ReqAngleFromN);

            if (BtweenAngle > 10)
            {
                Direction rotatedirection = GetRotateTargetDerection(ReqAngleFromN);
                if (rotatedirection == Direction.Right)
                {
                    RotationR(120);
                }
                else
                {
                    RotationL(120);
                }
                return true;
            }

            return false;
        }

        private Direction GetRotateTargetDerection(float ReqAngleFromN)
        {
            if (Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN) > 180)
            {
                if (CompassCore.Instance.AngleFromN < ReqAngleFromN)
                {
                    return Direction.Left;
                }
                else
                {
                    return Direction.Right;
                }
            }
            else
            {
                if (CompassCore.Instance.AngleFromN < ReqAngleFromN)
                {
                    return Direction.Right;
                }
                else
                {
                    return Direction.Left;
                }
            }
        }

        private float GetBetweenTargetAngle(float ReqAngleFromN)
        {
            float BtweenAngle = Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN);
            if (BtweenAngle > 180)
                BtweenAngle = 360 - BtweenAngle;

            return BtweenAngle;
        }

        #endregion

        #region BasicMoveMethods

        private void MoveFront(byte power)
        {
            carDivice.l_motorDIR = true;
            carDivice.r_motorDIR = true;
            carDivice.l_motorpower = power;
            carDivice.r_motorpower = power;
        }

        private void MoveFront(byte Rpower, byte Lpower)
        {
            carDivice.l_motorDIR = true;
            carDivice.r_motorDIR = true;
            carDivice.l_motorpower = Lpower;
            carDivice.r_motorpower = Rpower;
        }

        private void MoveBack(byte power)
        {
            carDivice.l_motorDIR = false;
            carDivice.r_motorDIR = false;
            carDivice.l_motorpower = power;
            carDivice.r_motorpower = power;
        }

        private void MoveBack(byte Rpower, byte Lpower)
        {
            carDivice.l_motorDIR = false;
            carDivice.r_motorDIR = false;
            carDivice.l_motorpower = Lpower;
            carDivice.r_motorpower = Rpower;
        }

        private void RotationR(byte power)
        {
            carDivice.l_motorDIR = false;
            carDivice.r_motorDIR = true;
            carDivice.l_motorpower = power;
            carDivice.r_motorpower = power;
        }

        private void RotationL(byte power)
        {
            carDivice.l_motorDIR = true;
            carDivice.r_motorDIR = false;
            carDivice.l_motorpower = power;
            carDivice.r_motorpower = power;
        }

        #endregion

        private float CalcReqAngleFromN(GPSsPosition MyPos, GPSsPosition TargetPos)
        {
            float x = Mathf.Abs(MyPos.longitude - TargetPos.longitude);
            float y = Mathf.Abs(MyPos.latitude - TargetPos.latitude);

            float ReqAngleFromN;

            if (MyPos.longitude == TargetPos.longitude && MyPos.latitude == TargetPos.latitude)//위치가 동일함
            {
                ReqAngleFromN = 0;
            }
            else if (MyPos.longitude == TargetPos.longitude)//x축 상에 위치
            {
                if (MyPos.latitude < TargetPos.latitude)//+
                {
                    ReqAngleFromN = 90;
                }
                else//-
                {
                    ReqAngleFromN = 270;
                }
            }
            else if (MyPos.latitude == TargetPos.latitude)//y축 상에 위치
            {
                if (MyPos.longitude < TargetPos.longitude)//+
                {
                    ReqAngleFromN = 0;
                }
                else//-
                {
                    ReqAngleFromN = 180;
                }
            }
            else
            {
                ReqAngleFromN = Mathf.Atan2(y, x) / Mathf.PI * 180;

                if (MyPos.longitude < TargetPos.longitude)//1, 4 분면에 위치
                {
                    if (MyPos.latitude < TargetPos.latitude)//1
                    {
                        ReqAngleFromN = 90 - ReqAngleFromN;
                    }
                    else//4
                    {
                        ReqAngleFromN = 90 + ReqAngleFromN;
                    }
                }
                else//2, 3 분면에 위치
                {
                    if (MyPos.latitude < TargetPos.latitude)//2
                    {
                        ReqAngleFromN = 270 + ReqAngleFromN;
                    }
                    else//3
                    {
                        ReqAngleFromN = 270 - ReqAngleFromN;
                    }
                }
            }

            return ReqAngleFromN;
        }
    }

    public enum MoveSequence
    {
        ForcedForward,//1-1
        SemiAutomaticForward,//1-2
        Rotat90To2n2,
        ProgressiveLengthMeasure,//2-2
        RetractionMeasure,//2-3
        Root//2-4
    }

    public enum ProgressiveLengthMeasureSubSequence
    {
        Front,
        Rotate180,
        Rotate90
    }

    public enum Direction
    {
        Right,
        Left
    }
}
