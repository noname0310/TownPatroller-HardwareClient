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
        public GPSSpotManager gPSSpotManager;
        private BaseCarDivice carDivice;
        public bool EnableTraceMode { get; set; }

        MoveSequence CurrentMoveSequence;
        SemiAutomaticSubSequence SemiAutomaticSubSequence;
        ProgressiveLengthMeasureSubSequence ProgressiveLengthMeasureSubSequence;
        ushort ShortestDistance;
        bool PrevChangeValue;
        int SetDistanceCount;
        Direction RotateDirection;
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
            gPSSpotManager = new GPSSpotManager(0);
            carDivice = baseCarDivice;
            EnableTraceMode = true;
            CurrentMoveSequence = MoveSequence.ForcedForward;

            carDivice.statusparser.OnParsedSOP += Statusparser_OnParsedSOP;
            gPSSpotManager.AddPos(new GPSPosition(null, -100, 100));
            gPSSpotManager.AddPos(new GPSPosition(null, 1221, 312));
        }

        private void Statusparser_OnParsedSOP()
        {
            if (EnableTraceMode && gPSSpotManager.GPSPositions.Count > 1)
            {
                IGConsole.Instance.Main.println(CurrentMoveSequence.ToString());
                //CompassCore.Instance.AngleFromN
                GPSsPosition MyPos = new GPSsPosition(0, 0);//GPSCore.Instance.GetGPSPosition().GetGPSS();
                GPSsPosition TargetPos = gPSSpotManager.GPSPositions[gPSSpotManager.CurrentMovePosIndex].GetGPSS();

                float x = Mathf.Abs(MyPos.longitude - TargetPos.longitude);
                float y = Mathf.Abs(MyPos.latitude - TargetPos.latitude);

                float Distance = Mathf.Sqrt(x * x + y * y);
                float ReqAngleFromN = CalcReqAngleFromN(MyPos, TargetPos);
                ReqAngleFromN = 360 - ReqAngleFromN;

                if (Distance < 20)
                {
                    gPSSpotManager.MoveNext();
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
                            else if (carDivice.f_sonardist < 20)
                            {
                                ShortestDistance = 20;
                                RotateDirection = Direction.Right;
                                SetDistanceCount = 0;
                                PrevChangeValue = false;
                                CurrentMoveSequence = MoveSequence.AngleMeasure;
                                goto case MoveSequence.AngleMeasure;
                            }
                            else if (carDivice.rs_sonardist < 20)
                            {
                                TraceDraction = Direction.Right;
                                ShortestDistance = 20;
                                RotateDirection = Direction.Right;
                                SetDistanceCount = 0;
                                PrevChangeValue = false;
                                SemiAutomaticSubSequence = SemiAutomaticSubSequence.GetTraceDractionAngle;
                                CurrentMoveSequence = MoveSequence.SemiAutomaticForward;
                                goto case MoveSequence.SemiAutomaticForward;
                            }
                            else if (carDivice.ls_sonardist < 20)
                            {
                                TraceDraction = Direction.Left;
                                ShortestDistance = 20;
                                RotateDirection = Direction.Right;
                                SetDistanceCount = 0;
                                PrevChangeValue = false;
                                SemiAutomaticSubSequence = SemiAutomaticSubSequence.GetTraceDractionAngle;
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
                        if (SemiAutomaticSubSequence == SemiAutomaticSubSequence.GetTraceDractionAngle)
                        {
                            if (SetDistanceCount < 5)
                            {
                                SetDistanceCount++;
                                MoveToPerpendicularAngle();
                            }
                            else
                            {
                                Direction rotatedirection = GetRotateTargetDerection(ReqAngleFromN);

                                if (rotatedirection == Direction.Right)
                                {
                                    if (TraceDraction == Direction.Left)
                                    {
                                        CurrentMoveSequence = MoveSequence.ForcedForward;
                                        goto case MoveSequence.ForcedForward;
                                    }
                                    else
                                    {
                                        SemiAutomaticSubSequence = SemiAutomaticSubSequence.TraceWall;
                                        goto case MoveSequence.SemiAutomaticForward;
                                    }

                                }
                                else//왼쪽에 ReqAngleFromN존재
                                {
                                    if (TraceDraction == Direction.Right)
                                    {
                                        CurrentMoveSequence = MoveSequence.ForcedForward;
                                        goto case MoveSequence.ForcedForward;
                                    }
                                    else
                                    {
                                        SemiAutomaticSubSequence = SemiAutomaticSubSequence.TraceWall;
                                        goto case MoveSequence.SemiAutomaticForward;
                                    }
                                }
                            }
                        }
                        else if (SemiAutomaticSubSequence == SemiAutomaticSubSequence.TraceWall)
                        {
                            BetweenAngle = GetBetweenTargetAngle(ReqAngleFromN);

                            if (BetweenAngle > 80)
                            {
                                Direction direction = GetRotateTargetDerection(ReqAngleFromN);

                                if (direction == Direction.Right)
                                {
                                    if (TraceDraction == Direction.Left)
                                    {
                                        CurrentMoveSequence = MoveSequence.ForcedForward;
                                        goto case MoveSequence.ForcedForward;
                                    }
                                    else
                                    {
                                        RBlocked = true;
                                        LBlocked = false;

                                        MoveFrontTickCount = 10;
                                        CurrentMoveFrontCount = 0;
                                        //TraceDraction = TraceDraction;

                                        CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                        goto case MoveSequence.ProgressiveLengthMeasure;
                                    }
                                }
                                else//왼쪽에 ReqAngleFromN존재
                                {
                                    if (TraceDraction == Direction.Right)
                                    {
                                        CurrentMoveSequence = MoveSequence.ForcedForward;
                                        goto case MoveSequence.ForcedForward;
                                    }
                                    else
                                    {
                                        RBlocked = false;
                                        LBlocked = true;

                                        MoveFrontTickCount = 10;
                                        CurrentMoveFrontCount = 0;
                                        //TraceDraction = TraceDraction;

                                        CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                        goto case MoveSequence.ProgressiveLengthMeasure;
                                    }
                                }
                            }
                            else
                            {
                                DriveWall(TraceDraction);
                            }
                        }
                        break;
                    #endregion

                    case MoveSequence.AngleMeasure:
                        #region 2-1
                        if (SetDistanceCount < 5)
                        {
                            SetDistanceCount++;
                            MoveToFrontPerpendicularAngle();
                            rotationSaved = false;
                        }
                        else
                        {
                            if (rotationSaved == false)//reqrotation 설정
                            {
                                rotationSaved = true;
                                Direction direction = GetRotateTargetDerection(reqrotation);

                                if (direction == Direction.Right)
                                {
                                    reqrotation = (CompassCore.Instance.AngleFromN - 90) % 360;
                                    TraceDraction = Direction.Left;
                                }
                                else//왼쪽에 ReqAngleFromN존재
                                {
                                    reqrotation = (CompassCore.Instance.AngleFromN + 90) % 360;
                                    TraceDraction = Direction.Right;
                                }
                            }
                            else
                            {
                                if (RotateToReqAngle(reqrotation) == false)
                                {
                                    RBlocked = false;
                                    LBlocked = false;

                                    MoveFrontTickCount = 10;
                                    CurrentMoveFrontCount = 0;
                                    //TraceDraction = TraceDraction;

                                    CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                    ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                    goto case MoveSequence.ProgressiveLengthMeasure;
                                }
                            }
                        }
                        break;
                    #endregion

                    case MoveSequence.ProgressiveLengthMeasure:
                        #region 2-2
                        BetweenAngle = GetBetweenTargetAngle(ReqAngleFromN);

                        if (BetweenAngle > 120 && ProgressiveLengthMeasureSubSequence != ProgressiveLengthMeasureSubSequence.Rotate180)
                        {
                            //TraceDraction = TraceDraction;
                            TurnBack = false; 
                            CurrentMoveSequence = MoveSequence.RetractionMeasure;
                            goto case MoveSequence.RetractionMeasure;
                        }
                        else if (BetweenAngle < 70 && ProgressiveLengthMeasureSubSequence != ProgressiveLengthMeasureSubSequence.Rotate180)
                        {
                            //TraceDraction = TraceDraction;
                            ShortestDistance = 20;
                            RotateDirection = Direction.Right;
                            SetDistanceCount = 0;
                            PrevChangeValue = false;
                            SemiAutomaticSubSequence = SemiAutomaticSubSequence.GetTraceDractionAngle;
                            CurrentMoveSequence = MoveSequence.SemiAutomaticForward;
                            goto case MoveSequence.SemiAutomaticForward;
                        }
                        else
                        {
                            switch (ProgressiveLengthMeasureSubSequence)
                            {
                                case ProgressiveLengthMeasureSubSequence.Front:
                                    if (carDivice.f_sonardist > 20)
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
                                            ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Rotate90;
                                            goto case ProgressiveLengthMeasureSubSequence.Rotate90;
                                        }
                                    }
                                    if (MoveFrontTickCount == CurrentMoveFrontCount)
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
                                    else
                                    {
                                        if (RotateToReqAngle(reqrotation) == false)
                                        {
                                            MoveFrontTickCount *= MoveFrontTickCount;
                                            CurrentMoveFrontCount = 0;
                                            TraceDraction = (TraceDraction == Direction.Right) ? Direction.Left : Direction.Right;
                                            ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                            goto case ProgressiveLengthMeasureSubSequence.Front;
                                        }
                                    }
                                    break;

                                case ProgressiveLengthMeasureSubSequence.Rotate90:
                                    if (rotationSaved == false)
                                    {
                                        rotationSaved = true; 
                                        
                                        Direction direction = GetRotateTargetDerection(reqrotation);

                                        if (direction == Direction.Right)
                                        {
                                            reqrotation = (CompassCore.Instance.AngleFromN - 90) % 360;
                                            TraceDraction = Direction.Left;
                                        }
                                        else//왼쪽에 ReqAngleFromN존재
                                        {
                                            reqrotation = (CompassCore.Instance.AngleFromN + 90) % 360;
                                            TraceDraction = Direction.Right;
                                        }
                                    }
                                    else
                                    {
                                        if (RotateToReqAngle(reqrotation) == false)
                                        {
                                            //XXXXXXXXgoto2-3
                                        }
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
                            if (carDivice.f_sonardist < 20)
                            {
                                MoveFront(0);
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

                                        MoveFrontTickCount = 10;
                                        CurrentMoveFrontCount = 0;
                                        //TraceDraction = TraceDraction;

                                        CurrentMoveSequence = MoveSequence.ProgressiveLengthMeasure;
                                        ProgressiveLengthMeasureSubSequence = ProgressiveLengthMeasureSubSequence.Front;
                                        goto case MoveSequence.ProgressiveLengthMeasure;
                                    }
                                    else//안쪽으로 들어갈때
                                    {
                                        MoveFront(0);
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

        public void ChangeSpot(GPSSpotManager gPSSpotManager)
        {

        }

        #region CalcMoveMethods

        private void DriveWall(Direction direction)
        {
            if (direction == Direction.Left)
            {
                if (carDivice.ls_sonardist > 20)
                {
                    MoveFront(255, 150);
                }
                else if (carDivice.ls_sonardist < 10)
                {
                    MoveFront(150, 255);
                }
                else
                {
                    MoveFront(255);
                }
            }
            else
            {
                if (carDivice.rs_sonardist > 20)
                {
                    MoveFront(150, 255);
                }
                else if (carDivice.rs_sonardist < 10)
                {
                    MoveFront(255, 150);
                }
                else
                {
                    MoveFront(255);
                }
            }
        }

        private void MoveToFrontPerpendicularAngle()
        {
            if (PrevChangeValue)
            {
                if (ShortestDistance < carDivice.f_sonardist)
                {
                    RotateDirection = (RotateDirection == Direction.Right) ? Direction.Left : Direction.Right;
                }
            }

            if (RotateDirection == Direction.Right)
                RotationR(90);
            else
                RotationL(90);
            PrevChangeValue = true;
        }

        private void MoveToPerpendicularAngle()
        {
            if(TraceDraction == Direction.Left)
            {
                if (PrevChangeValue)
                {
                    if (ShortestDistance < carDivice.ls_sonardist)
                    {
                        RotateDirection = (RotateDirection == Direction.Right) ? Direction.Left : Direction.Right;
                    }
                }

                if (RotateDirection == Direction.Right)
                    RotationR(90);
                else
                    RotationL(90);
                PrevChangeValue = true;
            }

            else if(TraceDraction == Direction.Right)
            {
                if (PrevChangeValue)
                {
                    if (ShortestDistance < carDivice.rs_sonardist)
                    {
                        RotateDirection = (RotateDirection == Direction.Right) ? Direction.Left : Direction.Right;
                    }
                }

                if (RotateDirection == Direction.Right)
                    RotationR(90);
                else
                    RotationL(90);
                PrevChangeValue = true;
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
                if (CompassCore.Instance.AngleFromN > ReqAngleFromN)
                {
                    return Direction.Right;
                }
                else
                {
                    return Direction.Left;
                }
            }
            else
            {
                if (CompassCore.Instance.AngleFromN > ReqAngleFromN)
                {
                    return Direction.Left;
                }
                else
                {
                    return Direction.Right;
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
            carDivice.l_motorDIR = true;
            carDivice.r_motorDIR = false;
            carDivice.l_motorpower = power;
            carDivice.r_motorpower = power;
        }

        private void RotationL(byte power)
        {
            carDivice.l_motorDIR = false;
            carDivice.r_motorDIR = true;
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
        AngleMeasure,//2-1
        ProgressiveLengthMeasure,//2-2
        RetractionMeasure,//2-3
        Root//2-4
    }

    public enum SemiAutomaticSubSequence
    {
        GetTraceDractionAngle,
        TraceWall
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
