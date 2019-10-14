using UnityEngine;
using TownPatroller.CarDevice;

public class ObjectCarDevice : MonoBehaviour
{
    public BaseCarDivice Basecardivice;
    public GameObject btCore;
    public GameObject StatusUI;

    private bool FBLSpeedSlow;
    private bool FBRSpeedSlow;
    private byte Al_motorpower;
    private byte Ar_motorpower;

    private bool SideSpeedSlow;

    void Awake()
    {
        btCore = GameObject.Find("BTManager");
        StatusUI = GameObject.Find("CarStatusPanel");
        Basecardivice = new BaseCarDivice(btCore, StatusUI);
        Basecardivice.statusparser.OnParsedEOP += Statusparser_OnParsedEOP;
        FBLSpeedSlow = false;
        FBRSpeedSlow = false;
        SideSpeedSlow = false;
    }

    private void Statusparser_OnParsedEOP()
    {
        if (Basecardivice.HalfManualMode == true)
        {
            if (Basecardivice.l_motorpower > 0 && Basecardivice.r_motorpower > 0)
            {
                if (Basecardivice.l_motorDIR == true && Basecardivice.r_motorDIR == true)//전진
                {
                    if (Basecardivice.f_sonardist < 20)
                    {
                        CarStop();
                    }

                    else if (Basecardivice.rs_sonardist < 20 && Basecardivice.ls_sonardist < 20)
                    {

                    }

                    else
                    {
                        if(Basecardivice.rs_sonardist < 20 && FBLSpeedSlow == false)
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower /= 2;
                            FBLSpeedSlow = true;
                        }

                        else if (Basecardivice.rs_sonardist >= 20 && FBLSpeedSlow == true)
                        {
                            Basecardivice.r_motorpower = Ar_motorpower;
                            FBLSpeedSlow = false;
                        }


                        if (Basecardivice.ls_sonardist < 20 && FBRSpeedSlow == false)
                        {
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower /= 2;
                            FBRSpeedSlow = true;
                        }

                        else if (Basecardivice.ls_sonardist >= 20 && FBRSpeedSlow == true)
                        {
                            Basecardivice.l_motorpower = Al_motorpower;
                            FBRSpeedSlow = false;
                        }
                    }
                }

                else if (Basecardivice.l_motorDIR == false && Basecardivice.r_motorDIR == false)//후진
                {
                    if (!(Basecardivice.rs_sonardist < 20 && Basecardivice.ls_sonardist < 20))
                    {
                        if (Basecardivice.rs_sonardist < 20 && FBLSpeedSlow == false)
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower /= 2;
                            FBLSpeedSlow = true;
                        }

                        else if (Basecardivice.rs_sonardist >= 20 && FBLSpeedSlow == true)
                        {
                            Basecardivice.r_motorpower = Ar_motorpower;
                            FBLSpeedSlow = false;
                        }


                        if (Basecardivice.ls_sonardist < 20 && FBRSpeedSlow == false)
                        {
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower /= 2;
                            FBRSpeedSlow = true;
                        }

                        else if (Basecardivice.ls_sonardist >= 20 && FBRSpeedSlow == true)
                        {
                            Basecardivice.l_motorpower = Al_motorpower;
                            FBRSpeedSlow = false;
                        }
                    }
                }
            }

            else
            {
                FBLSpeedSlow = false;
                FBRSpeedSlow = false;
            }

            if (Basecardivice.l_motorpower > 0 || Basecardivice.r_motorpower > 0)
            {
                if ((Basecardivice.l_motorDIR == true && Basecardivice.r_motorDIR == false)
                    || (Basecardivice.l_motorDIR == true && Basecardivice.l_motorpower > 0 && Basecardivice.r_motorpower == 0)
                    || (Basecardivice.r_motorDIR == false && Basecardivice.r_motorpower > 0 && Basecardivice.l_motorpower == 0))//우측회전
                {
                    if (Basecardivice.rs_sonardist < 15 && SideSpeedSlow == false)
                    {
                        if (Basecardivice.ls_sonardist < 15)//양쪽다 막힌경우
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower = 0;
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower = 0;
                        }
                        else//한쪽 막혀서 회전 힘듬
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower /= 2;
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower /= 2;
                        }
                        SideSpeedSlow = true;
                    }
                    else if (Basecardivice.rs_sonardist >= 15 && SideSpeedSlow == true)
                    {
                        Basecardivice.l_motorpower = Al_motorpower;
                        Basecardivice.r_motorpower = Ar_motorpower;
                        SideSpeedSlow = false;
                    }
                }

                else if ((Basecardivice.l_motorDIR == false && Basecardivice.r_motorDIR == true)
                    || (Basecardivice.l_motorDIR == false && Basecardivice.l_motorpower > 0 && Basecardivice.r_motorpower == 0)
                    || (Basecardivice.r_motorDIR == true && Basecardivice.r_motorpower > 0 && Basecardivice.l_motorpower == 0))//좌측회전
                {
                    if (Basecardivice.ls_sonardist < 15 && SideSpeedSlow == false)
                    {
                        if (Basecardivice.rs_sonardist < 15)//양쪽다 막힌경우
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower = 0;
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower = 0;
                        }
                        else//한쪽 막혀서 회전 힘듬
                        {
                            Ar_motorpower = Basecardivice.r_motorpower;
                            Basecardivice.r_motorpower /= 2;
                            Al_motorpower = Basecardivice.l_motorpower;
                            Basecardivice.l_motorpower /= 2;
                        }
                        SideSpeedSlow = true;
                    }
                    else if (Basecardivice.ls_sonardist >= 15 && SideSpeedSlow == true)
                    {
                        Basecardivice.l_motorpower = Al_motorpower;
                        Basecardivice.r_motorpower = Ar_motorpower;
                        SideSpeedSlow = false;
                    }
                }
            }

            else
            {
                SideSpeedSlow = false;
            }
        }
    }
    private void CarStop()
    {
        if (Basecardivice.l_motorpower != 0)
        {
            Basecardivice.l_motorpower = 0;
        }
        if (Basecardivice.r_motorpower != 0)
        {
            Basecardivice.r_motorpower = 0;
        }
    }
}
