using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Testing;


namespace DataAnalysis
{
    public class LogRankTest
    {
        private byte[] _survivalStatusGroup1;
        private double[] _survivalMonthsGroup1;
        private byte[] _survivalStatusGroup2;
        private double[] _survivalMonthsGroup2;

        // constructors
        public LogRankTest()
        {
            
        }

        public LogRankTest(byte[] survivalStatusGroup1, double[] survivalMonthsGroup1, byte[] survivalStatusGroup2,
            double[] survivalMonthsGroup2) 
        {
            _survivalStatusGroup1 = survivalStatusGroup1;
            _survivalMonthsGroup1 = survivalMonthsGroup1;
            _survivalStatusGroup2 = survivalStatusGroup2;
            _survivalMonthsGroup2 = survivalMonthsGroup2;
            
        }

        public double GetPvalue()
        {checkDataValidatiy();
            checkNullValuesOfInput();
            List<Patient> survivalGroup1 = new List<Patient>();
            List<Patient> survivalGroup2 = new List<Patient>();
            Action<byte[], double[], List<Patient>> createList = (survivalStatusGroup, survivalMonthsGroup, survivalGroup) =>
            {
                int j = survivalStatusGroup.Length;
                for (int i = 0; i < j; i++)
                {
                    survivalGroup.Add(new Patient(survivalStatusGroup[i], survivalMonthsGroup[i]));
                }
            };
            createList(_survivalStatusGroup1, _survivalMonthsGroup1, survivalGroup1);
            createList(_survivalStatusGroup2, _survivalMonthsGroup2, survivalGroup2);

            survivalGroup1 = survivalGroup1.OrderBy(m => m.survivalMonths).ToList();
            survivalGroup2 = survivalGroup2.OrderBy(m => m.survivalMonths).ToList();

            int survivalMaxMonth =
                (int) Math.Ceiling(survivalGroup1[^1].survivalMonths >= survivalGroup2[^1].survivalMonths
                    ? survivalGroup1[^1].survivalMonths : survivalGroup2[^1].survivalMonths);

            // get total expected event count
            (double expectedEventCountGroup1, double expectedEventCountGroup2) s = 
                getExpectedEventCount(survivalGroup1, survivalGroup2, survivalMaxMonth);

            int observedEventGroup1 = survivalGroup1.Count(m => m.survivalStatus == 1);
            int observedEventGroup2 = survivalGroup2.Count(m => m.survivalStatus == 1);

            // chisquare calculate
            double lr = Math.Pow(observedEventGroup1 - s.Item1, 2) / Math.Pow(s.Item1, 2) +
                        Math.Pow(observedEventGroup2 - s.Item2, 2) / Math.Pow(s.Item2, 2);

            // calculate pvalue;
            ChiSquareTest chiSquareTest = new ChiSquareTest(lr, 1);
            double pvalue = chiSquareTest.PValue;

            return pvalue;
        }

        public double GetPvalue(byte[] survivalStatusGroup1, double[] survivalMonthsGroup1, byte[] survivalStatusGroup2,
            double[] survivalMonthsGroup2)
        {
            this._survivalStatusGroup1 = survivalStatusGroup1;
            this._survivalMonthsGroup1 = survivalMonthsGroup1;
            this._survivalStatusGroup2 = survivalStatusGroup2;
            this._survivalMonthsGroup2 = survivalMonthsGroup2;
            return GetPvalue();
        }

        private void checkDataValidatiy()
        {
            Action<byte[], double[]> compareLength = (bytes, doubles) =>
            {
                if (bytes.Length != doubles.Length)
                    throw new Exception($" The length of {bytes} and {doubles} are not the same.");
            };
            try
            {
                compareLength(_survivalStatusGroup1, _survivalMonthsGroup1);

            }
            catch (NullReferenceException)
            {
                Console.WriteLine("没有有效的数据");
                throw;
            }

            try
            {
                compareLength(_survivalStatusGroup2, _survivalMonthsGroup2);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("没有有效的数据");
                throw;
            }
        }

        private void checkNullValuesOfInput()
        {
            Action<byte[]> checkSurvivalStatusNull = bytes =>
            {
                if (bytes.Length == 0)
                {
                    throw new Exception($"{bytes} doesn't have any items.");
                }
            };
            Action<double[]> checkSurvivalMonthsNull = ints =>
            {
                if (ints.Length == 0)
                {
                    throw new Exception($"{ints} doesn't have any items.");
                }
            };
            checkSurvivalStatusNull(_survivalStatusGroup1);
            checkSurvivalMonthsNull(_survivalMonthsGroup1);
            checkSurvivalStatusNull(_survivalStatusGroup2);
            checkSurvivalMonthsNull(_survivalMonthsGroup2);
        }

        private (double, double) getExpectedEventCount(List<Patient> group1, List<Patient> group2, int survivalMonthMax)
        {
            double expectedEventCountGroup1 = 0D;
            double expectedEventCountGroup2 = 0D;
            int deathCountGroup1 = 0;
            int deathCountGroup2 = 0;
            int aliveTotalNumberGroup1 = group1.Count;
            int aliveTotalNumberGroup2 = group2.Count;

            for (int i = 1; i <= survivalMonthMax; i++)
            {
                aliveTotalNumberGroup1 -= deathCountGroup1;
                aliveTotalNumberGroup2 -= deathCountGroup2;
                deathCountGroup1 = group1.Count(m => m.survivalMonths <= i && m.survivalMonths > i - 1);
                deathCountGroup2 = group2.Count(m => m.survivalMonths <= i && m.survivalMonths > i - 1);
                expectedEventCountGroup1 += (double) (deathCountGroup1 + deathCountGroup2) * aliveTotalNumberGroup1 /
                                            (aliveTotalNumberGroup1 + aliveTotalNumberGroup2);
                expectedEventCountGroup2 += (double) (deathCountGroup1 + deathCountGroup2) * aliveTotalNumberGroup2 /
                                            (aliveTotalNumberGroup1 + aliveTotalNumberGroup2);
            }

            return (expectedEventCountGroup1, expectedEventCountGroup2);
        }
    }

    internal class Patient
    {
        internal byte survivalStatus;
        internal double survivalMonths;

        // constructors
        internal Patient(byte survivalStatus, double survivalMonths)
        {
            this.survivalStatus = survivalStatus;
            this.survivalMonths = survivalMonths;
        }
    }
}