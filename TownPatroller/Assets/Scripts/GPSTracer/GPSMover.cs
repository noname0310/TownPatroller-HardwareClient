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
        ushort ShortestDistance;
        bool PrevChangeValue;
        int SetDistanceCount;
        Direction RotateDirection;
        Direction TraceDraction;

        public void _new(BaseCarDivice baseCarDivice)
        {
            gPSSpotManager = new GPSSpotManager(0);
            carDivice = baseCarDivice;
            EnableTraceMode = true;
            CurrentMoveSequence = MoveSequence.ForcedForward;

            carDivice.statusparser.OnParsedSOP += Statusparser_OnParsedSOP;
            gPSSpotManager.AddPos(new GPSPosition(null, 100, 100));
            gPSSpotManager.AddPos(new GPSPosition(null, 1221, 312));
        }

        private void Statusparser_OnParsedSOP()
        {
            if (EnableTraceMode && gPSSpotManager.GPSPositions.Count > 1)
            {
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

                    case MoveSequence.SemiAutomaticForward:
                        if (SemiAutomaticSubSequence == SemiAutomaticSubSequence.GetTraceDractionAngle)
                        {
                            if (SetDistanceCount < 5)
                            {
                                SetDistanceCount++;
                                MoveToPerpendicularAngle();
                            }
                            else
                            {
                                if (Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN) > 180)
                                {
                                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)//오른쪽에 ReqAngleFromN존재
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
                                else
                                {
                                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)//왼쪽에 ReqAngleFromN존재
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
                                    else//오른쪽에 ReqAngleFromN존재
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
                                }
                            }
                        }
                        else if (SemiAutomaticSubSequence == SemiAutomaticSubSequence.TraceWall)
                        {
                            float BtweenAngle = Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN);
                            if (BtweenAngle > 180)
                                BtweenAngle = 360 - BtweenAngle;

                            if (BtweenAngle > 60)
                            {
                                if (Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN) > 180)
                                {
                                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)//오른쪽에 ReqAngleFromN존재
                                    {
                                        if (TraceDraction == Direction.Left)
                                        {
                                            CurrentMoveSequence = MoveSequence.ForcedForward;
                                            goto case MoveSequence.ForcedForward;
                                        }

                                    }
                                    else//왼쪽에 ReqAngleFromN존재
                                    {
                                        if (TraceDraction == Direction.Right)
                                        {
                                            CurrentMoveSequence = MoveSequence.ForcedForward;
                                            goto case MoveSequence.ForcedForward;
                                        }
                                    }
                                }
                                else
                                {
                                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)//왼쪽에 ReqAngleFromN존재
                                    {
                                        if (TraceDraction == Direction.Right)
                                        {
                                            CurrentMoveSequence = MoveSequence.ForcedForward;
                                            goto case MoveSequence.ForcedForward;
                                        }
                                    }
                                    else//오른쪽에 ReqAngleFromN존재
                                    {
                                        if (TraceDraction == Direction.Left)
                                        {
                                            CurrentMoveSequence = MoveSequence.ForcedForward;
                                            goto case MoveSequence.ForcedForward;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (TraceDraction == Direction.Left)
                                {
                                    if (carDivice.ls_sonardist > 20)
                                    {
                                        MoveFront(255, 150);
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
                                    else
                                    {
                                        MoveFront(255);
                                    }
                                }
                            }
                        }
                        break;

                    case MoveSequence.AngleMeasure:
                        break;

                    case MoveSequence.ProgressiveLengthMeasure:
                        break;

                    case MoveSequence.RetractionMeasure:
                        break;

                    case MoveSequence.Root:
                        break;

                    default:
                        break;
                }
            }
        }

        public void ChangeSpot(GPSSpotManager gPSSpotManager)
        {

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
            float BtweenAngle = Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN);
            if (BtweenAngle > 180)
                BtweenAngle = 360 - BtweenAngle;

            if (BtweenAngle > 10)
            {
                if (Mathf.Abs(CompassCore.Instance.AngleFromN - ReqAngleFromN) > 180)
                {
                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)
                    {
                        RotationR(120);
                    }
                    else
                    {
                        RotationL(120);
                    }
                }
                else
                {
                    if (CompassCore.Instance.AngleFromN > ReqAngleFromN)
                    {
                        RotationL(120);
                    }
                    else
                    {
                        RotationR(120);
                    }
                }
                return true;
            }

            return false;
        }

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

    public enum Direction
    {
        Right,
        Left
    }
}
