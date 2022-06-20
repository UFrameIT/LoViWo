using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;

public class TestCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        test();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void test()
    {
        string[] variables = { "mtr_av", "sft_1_av", "c_1_av", "c_2_av", "sft_2_av", "c_3_av", "dev_1_av", "dev_2_av", "mtr_drive", "sft_1_drive", "c_1_drive", "c_2_drive", "sft_2_drive", "c_3_drive", "dev_1_drive", "dev_2_drive" };

        List<double[]> mat_1_rows = new List<double[]>();
        {
            double[] row_av_1 = { 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_1);
            double[] row_av_2 = { 0, 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_2);
            double[] row_av_3 = { 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_3);
            double[] row_av_4 = { 0, 0, 0, -1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_4);
            double[] row_av_5 = { 0, 0, 0, 0, 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_5);
            double[] row_av_6 = { 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_6);
            double[] row_av_7 = { 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_av_7);

            double[] row_av_F_1 = { 0, 0, 0, 0, 0, 0, -10, 0, 0, 0, 0, 0, 0, 0, 1, 0 };
            mat_1_rows.Add(row_av_F_1);
            double[] row_av_F_2 = { 0, 0, 0, 0, 0, 0, 0, -20, 0, 0, 0, 0, 0, 0, 0, 1 };
            mat_1_rows.Add(row_av_F_2);

            double[] row_F_1 = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_F_1);
            double[] row_F_2 = { 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_F_2);
            double[] row_F_3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, 0, 0, 0, 0 };
            mat_1_rows.Add(row_F_3);
            double[] row_F_4 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, -0.25, 0, 0, 0, 0 };
            mat_1_rows.Add(row_F_4);
            double[] row_F_5 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 1, 0, 0, 0 };
            mat_1_rows.Add(row_F_5);
            double[] row_F_6 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, -1 };
            mat_1_rows.Add(row_F_6);
            double[] row_F_7 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, -0.25 };
            mat_1_rows.Add(row_F_7);
        }
        Matrix<double> matrix_1_A = Matrix<double>.Build.DenseOfRows(mat_1_rows);

        double[] b_1_collumn = { 0,0,0,0,0,0,0, 0,0, 100,0,0,0,0,0,0};

        Vector<double> vector_1_B = Vector<double>.Build.DenseOfEnumerable(b_1_collumn);

        int rank_1_A = matrix_1_A.Rank();

        Matrix<double> matrix_1_AExtendet = matrix_1_A.InsertColumn(matrix_1_A.ColumnCount, vector_1_B);

        int rank_1_AExtendet = matrix_1_AExtendet.Rank();

        Debug.Log("matrixA 1: " );
        foreach(double[] row in matrix_1_A.ToRowArrays())
        {
            Debug.Log(string.Join("   ", row));
        }
        Debug.Log("matrixAExtendet 1: " );
        foreach (double[] row in matrix_1_AExtendet.ToRowArrays())
        {
            Debug.Log(string.Join("   ", row));
        }

        Debug.Log("rankA 1: " + rank_1_A);
        Debug.Log("rankAExtendet 1: " + rank_1_AExtendet);

        Vector<double> gls_1_Solution = matrix_1_A.Solve(vector_1_B);
        //Vector<double> glsIterativeSolution = matrixA.SolveIterative(vectorB, new MlkBiCgStab());

        Debug.Log("Solution 1: " + gls_1_Solution.ToString());
        //Debug.Log("Solution: " + glsIterativeSolution.ToString());





        List<double[]> mat_2_rows = new List<double[]>();
        {
            double[] row_av_1 = { 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_1);
            double[] row_av_2 = { 0, 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_2);
            double[] row_av_3 = { 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_3);
            double[] row_av_4 = { 0, 0, 0, -1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_4);
            double[] row_av_5 = { 0, 0, 0, 0, 1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_5);
            double[] row_av_6 = { 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_6);
            double[] row_av_7 = { 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_av_7);

            double[] row_av_F_1 = { 0, 0, 0, 0, 0, 0, -10, 0, 0, 0, 0, 0, 0, 0, 1, 0 };
            mat_2_rows.Add(row_av_F_1);
            double[] row_av_F_2 = { 0, 0, 0, 0, 0, 0, 0, -20, 0, 0, 0, 0, 0, 0, 0, 1 };
            mat_2_rows.Add(row_av_F_2);

            double[] row_F_1 = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_F_1);
            double[] row_F_2 = { 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, 0, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_F_2);
            double[] row_F_3 = { 0, 0, 0, 0, 0, 0, 0, 0, 1, -2, 1, 0, 0, 0, 0, 0 };
            mat_2_rows.Add(row_F_3);
            double[] row_F_4 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 1, -0.25, 0, 0, 0, 0 };
            mat_2_rows.Add(row_F_4);
            double[] row_F_5 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -0.5, -0.5, 0.25, 0, 0, 0 };
            mat_2_rows.Add(row_F_5);
            double[] row_F_6 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -2, 1, 1, 0 };
            mat_2_rows.Add(row_F_6);
            double[] row_F_7 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -1 };
            mat_2_rows.Add(row_F_7);
            double[] row_F_8 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, -1, 0, -0.25 };
            mat_2_rows.Add(row_F_8);
            double[] row_F_9 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, 0.25 };
            mat_2_rows.Add(row_F_9);
        }
        Matrix<double> matrix_2_A = Matrix<double>.Build.DenseOfRows(mat_2_rows);

        double[] b_2_collumn = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0 };

        Vector<double> vector_2_B = Vector<double>.Build.DenseOfEnumerable(b_2_collumn);

        int rank_2_A = matrix_2_A.Rank();

        Matrix<double> matrix_2_AExtendet = matrix_2_A.InsertColumn(matrix_2_A.ColumnCount, vector_2_B);

        int rank_2_AExtendet = matrix_2_AExtendet.Rank();

        Debug.Log("matrixA 2: ");
        foreach (double[] row in matrix_2_A.ToRowArrays())
        {
            Debug.Log(string.Join("   ", row));
        }
        Debug.Log("matrixAExtendet 2: ");
        foreach (double[] row in matrix_2_AExtendet.ToRowArrays())
        {
            Debug.Log(string.Join("   ", row));
        }

        Debug.Log("rankA 2: " + rank_2_A);
        Debug.Log("rankAExtendet 2: " + rank_2_AExtendet);

        Vector<double> gls_2_Solution = matrix_2_A.Solve(vector_2_B);
        //Vector<double> glsIterativeSolution = matrixA.SolveIterative(vectorB, new MlkBiCgStab());

        Debug.Log("Solution 2: " + gls_2_Solution.ToString());
        //Debug.Log("Solution: " + glsIterativeSolution.ToString());
    }


}
