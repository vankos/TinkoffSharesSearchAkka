using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;

namespace Tinkoff
{
    static  class Hurst
    { 
        static double M,R, RSsum,RS1, RS2, RS3, RS4, RS5, RS6, RS7, RS8, RS9, RS10, RS11,LogRS1, LogRS2, LogRS3, LogRS4, LogRS5, LogRS6, LogRS7, LogRS8, LogRS9,
         LogRS10, LogRS11, E1, E2, E3, E4, E5, E6, E7, E8, E9, E10, E11;
        static List<double> DevAccum = new List<double>(1000);
        static List<double> StdDevMas = new List<double>(1000);
        static List<double> LogReturns;
        static List<double> rs1 = new List<double>(101);
        static List<double> rs2 = new List<double>(51);
        static List<double> rs3 = new List<double>(41);
        static List<double> rs4 = new List<double>(26);
        static List<double> rs5 = new List<double>(21);
        static List<double> rs6 = new List<double>(11);
        static List<double> rs7 = new List<double>(9);
        static List<double> rs8 = new List<double>(6);
        static List<double> rs9 = new List<double>(5);
        static List<double> rs10 = new List<double>(3);
        static List<double> rs11= new List<double>(1);
        static public double CalculateHurst(List<CandlePayload> candles)
        {
           
            List<decimal> close = CopyClose(candles);
            if (close.Count < 10000)
                throw new ApplicationException("Свечей должно быть больше тысячи");
            LogReturns = close.Select(closeValue => Math.Log((double)closeValue)).ToList();
            int num1 = 10, num2 = 20, num3 = 25, num4 = 40, num5 = 50, num6 = 100, num7 = 125, num8 = 200, num9 = 250, num10 = 500, num11 = 1000;
            for (int A = 1; A < 11; A++)
            {
                switch (A)
                {
                    case (1):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 100; j++)
                            {
                                rs1[j] = RSculc(10 * j - 9, 10 * j, 10);
                                RSsum += rs1[j];
                            }
                            RS1 = RSsum / 100;
                            LogRS1 = Math.Log(RS1);
                            break;
                        }
                    case (2):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 50; j++)
                            {
                                rs2[j] = RSculc(20 * j - 19, 20 * j, 20);
                                RSsum += rs2[j];
                            }
                            RS2 = RSsum / 50;
                            LogRS2 = Math.Log(RS2);
                            break;
                        }
                    case (3):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 40; j++)
                            {
                                rs3[j] = RSculc(25 * j - 24, 25 * j, 25);
                                RSsum += rs3[j];
                            }
                            RS3 = RSsum / 40;
                            LogRS3 = Math.Log(RS3);
                            break;
                        }
                    case (4):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 25; j++)
                            {
                                rs4[j] = RSculc(40 * j - 39, 40 * j, 40);
                                RSsum += rs4[j];
                            }
                            RS4 = RSsum / 25;
                            LogRS4 = Math.Log(RS4);
                            break;
                        }
                    case (5):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 20; j++)
                            {
                                rs5[j] = RSculc(50 * j - 49, 50 * j, 50);
                                RSsum += rs5[j];
                            }
                            RS5 = RSsum / 20;
                            LogRS5 = Math.Log(RS5);
                            break;
                        }
                    case (6):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 10; j++)
                            {
                                rs6[j] = RSculc(100 * j - 99, 100 * j, 100);
                                RSsum += rs6[j];
                            }
                            RS6 = RSsum / 10;
                            LogRS6 = Math.Log(RS6);
                            break;
                        }
                    case (7):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 8; j++)
                            {
                                rs7[j] = RSculc(125 * j - 124, 125 * j, 125);
                                RSsum += rs7[j];
                            }
                            RS7 = RSsum / 8;
                            LogRS7 = Math.Log(RS7);
                            break;
                        }
                    case (8):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 5; j++)
                            {
                                rs8[j] = RSculc(200 * j - 199, 200 * j, 200);
                                RSsum += rs8[j];
                            }
                            RS8 = RSsum / 5;
                            LogRS8 = Math.Log(RS8);
                            break;
                        }
                    case (9):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 4; j++)
                            {
                                rs9[j] = RSculc(250 * j - 249, 250 * j, 250);
                                RSsum += rs9[j];
                            }
                            RS9 = RSsum / 4;
                            LogRS9 = Math.Log(RS9);
                            break;
                        }
                    case (10):
                        {
                            RSsum = 0.0;
                            for (int j = 1; j <= 2; j++)
                            {
                                rs10[j] = RSculc(500 * j - 499, 500 * j, 500);
                                RSsum += rs10[j];
                            }
                            RS10 = RSsum / 2;
                            LogRS10 = Math.Log(RS10);
                            break;
                        }
                    case (11):
                        {

                            RS11 = RSculc(1, 1000, 1000);
                            LogRS11 = Math.Log(RS11);
                            break;
                        }
                }
            }

            return  RegCulc1000(LogRS1, LogRS2, LogRS3, LogRS4, LogRS5, LogRS6, LogRS7, LogRS8,
                 LogRS9, LogRS10, LogRS11);

            E1 = Math.Log(ERSculc(num1));
            E2 = Math.Log(ERSculc(num2));
            E3 = Math.Log(ERSculc(num3));
            E4 = Math.Log(ERSculc(num4));
            E5 = Math.Log(ERSculc(num5));
            E6 = Math.Log(ERSculc(num6));
            E7 = Math.Log(ERSculc(num7));
            E8 = Math.Log(ERSculc(num8));
            E9 = Math.Log(ERSculc(num9));
            E10 = Math.Log(ERSculc(num10));
            E11 = Math.Log(ERSculc(num11));

            return RegCulc1000(E1, E2, E3, E4, E5, E6, E7, E8, E9, E10, E11);    

        }

        static double RegCulc1000(double Y1, double Y2, double Y3, double Y4, double Y5, double Y6,
                   double Y7, double Y8, double Y9, double Y10, double Y11)
        {
            double SumY = 0.0;
            double SumX = 0.0;
            double SumYX = 0.0;
            double SumXX = 0.0;
            double b = 0.0;
            double[] N =new double[10];                                                    //array to store the divider logarithms
            double[] n = { 10, 20, 25, 40, 50, 100, 125, 200, 250, 500, 1000 };           //divider array
            //---Calculate N ratios
            for (int i = 0; i <= 10; i++)
            {
                N[i] = Math.Log(n[i]);
                SumX = SumX + N[i];
                SumXX = SumXX + N[i] * N[i];
            }
            SumY = Y1 + Y2 + Y3 + Y4 + Y5 + Y6 + Y7 + Y8 + Y9 + Y10 + Y11;
            SumYX = Y1 * N[0] + Y2 * N[1] + Y3 * N[2] + Y4 * N[3] + Y5 * N[4] + Y6 * N[5] + Y7 * N[6] + Y8 * N[7] + Y9 * N[8] + Y10 * N[9] + Y11 * N[10];

            //---Calculate the Beta regression ratio or the necessary Hurst exponent 
            b = (11 * SumYX - SumY * SumX) / (11 * SumXX - SumX * SumX);
            return (b);
        }

        static double ERSculc(double m)                 //m - 1000 dividers
        {
            double e;
            double nSum = 0.0;
            double part = 0.0;
            for (int i = 1; i <= m - 1; i++)
            {
                part = Math.Pow(((m - i) / i), 0.5);
                nSum = nSum + part;
            }
            e = Math.Pow((m * Math.PI / 2), -0.5) * nSum;
            return (e);
        }



        static double RSculc(int bottom, int top, int barscount)
        {
            double Sum=0.0,DevSum = 0.0, MinValue = 1000.0, MaxValue = 0.0, S1,RS;

            for (int i = bottom; i <= top; i++)
                Sum += LogReturns[i];
            M = Sum / barscount;

      
            for (int i = bottom; i <= top; i++)
            {
                DevAccum[i] = LogReturns[i] - M + DevAccum[i - 1];
                StdDevMas[i] = Math.Pow(LogReturns[i] - M, 2);
                DevSum += StdDevMas[i];
                if (DevAccum[i] > MaxValue)
                    MaxValue = DevAccum[i];

                if (DevAccum[i] < MinValue)      
                    MinValue = DevAccum[i];
            }

            R = MaxValue - MinValue;
            S1 = Math.Sqrt(DevSum / barscount);
            return R / S1;
        }

        private static List<decimal> CopyClose(List<CandlePayload> candles)
        {
           
            List<decimal> returnList = new List<decimal>();
            foreach (var candle in candles)
            {
                returnList.Add(candle.Close);
            }
            return returnList;
        }
    }
}
