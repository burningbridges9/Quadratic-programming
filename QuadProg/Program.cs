using System;
using System.Collections.Generic;
using System.Linq;

namespace QuadProg
{
    class Program
    {
        public const int qColIndex = 12;
        public const int rowSize = 11;
        public const int colSize = 13;

        class Label
        {
            public List<string> colStr { get; set; } = new List<string> { "z1", "z2", "z3", "z4", "z5", "z6", "z7", "z8", "z9", "z10", "z11", "z0", "q" };
            public List<string> rowStr { get; set; } = new List<string> { "w1", "w2", "w3", "w4", "w5", "w6", "w7", "w8", "w9", "w10", "w11" };
            public Label()
            {
                colStr = new List<string> { "z1", "z2", "z3", "z4", "z5", "z6", "z7", "z8", "z9", "z10", "z11", "z0", "q" };
                rowStr = new List<string> { "w1", "w2", "w3", "w4", "w5", "w6", "w7", "w8", "w9", "w10", "w11" };
            }


            public string SwapChars(int colIndex, int rowIndex)
            {
                var r = new string(this.rowStr[rowIndex]);
                var c = new string(this.colStr[colIndex]);
                rowStr[rowIndex] = c;
                colStr[colIndex] = r;
                return r;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var matrix = new List<List<double>>()
            {
                new List<double>() { -2, 0,     4, 6, 2, 4, 6, 2, 8, 10, 6,      -1,      0 },
                new List<double>() {  0, -2,    -1, -3, -3, 2, 0, 0, 2, 0, 0,             -1,      0 },
                new List<double>() { -4, 1,      0, 0, 0, 0, 0, 0, 0, 0, 0,             -1,      -1 },
                new List<double>() { -6, 3,      0, 0, 0, 0, 0, 0, 0, 0, 0,             -1,      -1 },
                new List<double>() { -2, 3,      0, 0, 0, 0, 0, 0, 0, 0, 0,             -1,      -1 },
                new List<double>() { -4, -2,      0, 0, 0, 0, 0, 0, 0, 0, 0,              -1,      -1 },
                new List<double>() { -6, 0,      0, 0, 0, 0, 0, 0, 0, 0, 0,              -1,      -1 },
                new List<double>() { -2, 0,      0, 0, 0, 0, 0, 0, 0, 0, 0,              -1,      -1 },
                new List<double>() { -8, -2,      0, 0, 0, 0, 0, 0, 0, 0, 0,              -1,      -1 },
                new List<double>() { -10, 0,      0, 0, 0, 0, 0, 0, 0, 0, 0,             -1,      -1 },
                new List<double>() { -6, 0,      0, 0, 0, 0, 0, 0, 0, 0, 0,              -1,      -1 },

            };
            var label = new Label();
            Print(matrix, label);
            var tpl = FirstStep(matrix, label);
            var matrixAfterFirstStep = tpl.Item1;
            var basisIndexToChangeInRow = tpl.Item2;
            var changedBasisChar = tpl.Item3;
            Print(matrixAfterFirstStep, label);
            Console.WriteLine($"changedBasisChar is {changedBasisChar}");
            var prevMatr = matrixAfterFirstStep;
            while (changedBasisChar != "z0")
            {
                var pair = BaseStep(prevMatr, label, changedBasisChar);
                var newMatr = pair.Item1;
                var newChangedBasisChar = pair.Item2;
                Console.WriteLine($"newChangedBasisChar is {newChangedBasisChar}");
                Print(newMatr, label);

                prevMatr = newMatr;
                changedBasisChar = newChangedBasisChar;
            }
            Console.ReadKey();
        }

        static (List<List<double>>, int, string) FirstStep(List<List<double>> matrix, Label label)
        {
            var newMatrix = InitNewMatrix(matrix);
            var qVals = matrix.Select(x => x[qColIndex]).ToList();
            var qMinVal = qVals.Min();
            var qMinValIndex = qVals.IndexOf(qMinVal);

            var changedBasisChar = label.SwapChars(qColIndex - 1, qMinValIndex);

            for (int i = 0; i < rowSize; i++)
            {
                if (i != qMinValIndex)
                    newMatrix[i][qColIndex] = matrix[i][qColIndex] - qMinVal;
                else
                    newMatrix[i][qColIndex] = -qMinVal;
            }

            for (int i = 0; i < rowSize; i++)
            {
                for (int j = 0; j < colSize; j++)
                {
                    if (i == qMinValIndex)
                    {
                        var m_sj = matrix[i][j] * (-1);
                        newMatrix[i][j] = m_sj;
                    }
                    else
                    {
                        var m_sj = matrix[qMinValIndex][j] * (-1);
                        newMatrix[i][j] = matrix[i][j] + m_sj;
                    }
                }
            }

            return (newMatrix, qMinValIndex, changedBasisChar);
        }

        static (List<List<double>>, string) BaseStep(List<List<double>> matrix, Label label, string changedChar)
        {
            var colToWatchIndex = changedChar[0] == 'w' ? label.colStr.IndexOf($"z{int.Parse(changedChar.Split('w')[1])}") : label.colStr.IndexOf($"w{int.Parse(changedChar.Split('z')[1])}");

            var colToWatchValues = GetCol(colToWatchIndex, matrix);
            var colQValues = GetCol(qColIndex, matrix);
            var minPair = MinQ_iM_is(colQValues, colToWatchValues);
            int minValIndex = minPair.Item2;
            double minVal = minPair.Item1;
            var newMatrix = InitNewMatrix(matrix);

            var changedBasisChar = label.SwapChars(colToWatchIndex, minValIndex);

            for (int j = 0; j < colSize; j++)
            {
                var m_sj = matrix[minValIndex][j] / minVal;
                newMatrix[minValIndex][j] = m_sj;
            }

            for (int i = 0; i < rowSize; i++)
            {
                for (int j = 0; j < colSize; j++)
                {
                    if (i != minValIndex)
                    {
                        var valToMult = matrix[i][colToWatchIndex];
                        var m_si = newMatrix[minValIndex][j];
                        var m_ij = matrix[i][j];
                        var res = m_ij - valToMult * m_si;
                        newMatrix[i][j] = res;
                    }
                }
            }

            return (newMatrix, changedBasisChar);
        }

        static void Print(List<List<double>> matrix, Label label)
        {
            var header = "|Basis | ";
            foreach (var s in label.colStr)
            {
                header += $" {s}  | ";
            }
            Console.WriteLine(header);
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            for (int i = 0; i < rowSize; i++)
            {
                var str = label.rowStr[i].Length < 3 ? $"| { label.rowStr[i]} | " : $"| { label.rowStr[i]}  | ";
                for (int j = 0; j < colSize; j++)
                {
                    str += matrix[i][j].ToString().Length < 3 ? $"  {matrix[i][j]}  | " : $" {matrix[i][j]} | ";
                }

                Console.WriteLine(str);
                Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            }

            Console.WriteLine("\n\n");
        }


        static List<double> GetCol(int colIndex, List<List<double>> matrix)
        {
            var retVal = new List<double>();
            matrix.ForEach(x => retVal.Add(x[colIndex]));
            return retVal;
        }

        static (double, int) MinQ_iM_is(List<double> qs, List<double> ms)
        {
            var minimum = Math.Pow(10, -10);
            var minVal = double.MaxValue;
            var minIndex = -1;
            for (int i = 0; i < ms.Count; i++)
            {
                if (ms[i] > minimum)
                {
                    if (qs[i] / ms[i] < minVal)
                    {
                        minVal = ms[i];
                        minIndex = i;
                    }
                }
            }
            return (minVal, minIndex);
        }

        private static List<List<double>> InitNewMatrix(List<List<double>> matrix)
        {
            var newMatrix = new List<List<double>>();
            for (int i = 0; i < rowSize; i++)
            {
                var l = new List<double>();
                for (int j = 0; j < colSize; j++)
                    l.Add(0);
                newMatrix.Add(l);
            }
            return newMatrix;
        }
    }
}
