private void button1_Click(object sender, EventArgs e)
{
    try
    {
        var model = ParseInput(redtInput.Text);
        var solution = SimplexSolver.Solve(model);

        MessageBox.Show($"Optimal Value: {solution.OptimalValue}\n" +
                        $"Variable Values: {string.Join(", ", solution.VariableValues)}");

        // Clear previous DataGridView content
        dataGridViewTableau.Columns.Clear();
        dataGridViewTableau.Rows.Clear();

        int numColumns = model.ObjectiveFunctionCoefficients.Count + model.Constraints.Count + 1; // +1 for RHS
        string[] columnNames = GetCustomColumnNames(numColumns);

        for (int i = 0; i < columnNames.Length; i++)
        {
            dataGridViewTableau.Columns.Add($"Column{i}", columnNames[i]);
        }

        // Populate the DataGridView with tableau data
        double[,] tableau = GetTableauData(model);
        for (int i = 0; i < tableau.GetLength(0); i++)
        {
            DataGridViewRow row = new DataGridViewRow();
            for (int j = 0; j < tableau.GetLength(1); j++)
            {
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = tableau[i, j] });
            }
            dataGridViewTableau.Rows.Add(row);
        }

        // Display optimal value permanently in a label
        lblOptimalValue.Text = $"Optimal Value: {solution.OptimalValue}";
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}\n\nExample format:\n\n" +
                        "max +2 +3 +3 +5 +2 +4\n" +
                        "+11 +8 +6 +14 +10 +10 <= 40\n" +
                        "+7 +5 +3 +9 +6 +8 <= 30\n" +
                        "bin bin bin bin bin bin");
    }
}

public static class SimplexSolver
{
    public static Solution Solve(Model model)
    {
        int numVariables = model.ObjectiveFunctionCoefficients.Count;
        int numConstraints = model.Constraints.Count;
        double[,] tableau = new double[numConstraints + 1, numVariables + numConstraints + 1];

        // Fill in the tableau for the objective function
        for (int i = 0; i < numVariables; i++)
        {
            tableau[0, i] = -model.ObjectiveFunctionCoefficients[i]; // Coefficients for maximization
        }

        // Fill in the tableau for the constraints
        for (int i = 0; i < numConstraints; i++)
        {
            for (int j = 0; j < numVariables; j++)
            {
                tableau[i + 1, j] = model.Constraints[i].Coefficients[j];
            }
            tableau[i + 1, numVariables + i] = 1; // Slack variable for each constraint
            tableau[i + 1, numVariables + numConstraints] = model.Constraints[i].RightHandSide; // Right-hand side
        }

        // Perform the Simplex algorithm
        while (true)
        {
            int pivotCol = FindPivotColumn(tableau);
            if (pivotCol == -1) break; // Optimal solution found

            int pivotRow = FindPivotRow(tableau, pivotCol);
            if (pivotRow == -1) throw new Exception("Problem is unbounded.");

            Pivot(tableau, pivotRow, pivotCol);
        }

        return ExtractSolution(tableau, model);
    }

    private static int FindPivotColumn(double[,] tableau)
    {
        int pivotCol = -1;
        double minValue = 0;
        for (int j = 0; j < tableau.GetLength(1) - 1; j++) // Exclude RHS
        {
            if (tableau[0, j] < minValue)
            {
                minValue = tableau[0, j];
                pivotCol = j;
            }
        }
        return pivotCol;
    }

    private static int FindPivotRow(double[,] tableau, int pivotCol)
    {
        int pivotRow = -1;
        double minRatio = double.MaxValue;
        for (int i = 1; i < tableau.GetLength(0); i++) // Exclude objective row
        {
            if (tableau[i, pivotCol] > 0)
            {
                double ratio = tableau[i, tableau.GetLength(1) - 1] / tableau[i, pivotCol]; // RHS / pivot column
                if (ratio < minRatio)
                {
                    minRatio = ratio;
                    pivotRow = i;
                }
            }
        }
        return pivotRow;
    }

    private static void Pivot(double[,] tableau, int pivotRow, int pivotCol)
    {
        double pivotValue = tableau[pivotRow, pivotCol];
        for (int j = 0; j < tableau.GetLength(1); j++)
        {
            tableau[pivotRow, j] /= pivotValue; // Normalize pivot row
        }

        for (int i = 0; i < tableau.GetLength(0); i++)
        {
            if (i != pivotRow)
            {
                double factor = tableau[i, pivotCol];
                for (int j = 0; j < tableau.GetLength(1); j++)
                {
                    tableau[i, j] -= factor * tableau[pivotRow, j]; // Adjust other rows
                }
            }
        }
    }

    private static Solution ExtractSolution(double[,] tableau, Model model)
    {
        var solution = new Solution();
        solution.VariableValues = new List<double>(new double[model.ObjectiveFunctionCoefficients.Count]);

        // Get optimal value from the last column of the objective function row
        solution.OptimalValue = -tableau[0, tableau.GetLength(1) - 1];

        // Get variable values
        for (int i = 0; i < model.ObjectiveFunctionCoefficients.Count; i++)
        {
            bool isBasic = true;
            for (int j = 1; j < tableau.GetLength(0); j++)
            {
                if (Math.Abs(tableau[j, i] - 1) < 1e-5 && Math.Abs(tableau[j, tableau.GetLength(1) - 1]) > 1e-5)
                {
                    solution.VariableValues[i] = tableau[j, tableau.GetLength(1) - 1];
                    isBasic = false;
                    break;
                }
            }
            if (isBasic)
            {
                solution.VariableValues[i] = 0;
            }
        }

        // Handle binary variables
        for (int i = 0; i < model.VariableTypes.Count; i++)
        {
            if (model.VariableTypes[i] == VariableType.Binary && (solution.VariableValues[i] < 0 || solution.VariableValues[i] > 1))
            {
                throw new Exception($"Variable {i + 1} must be binary.");
            }
        }

        return solution;
    }
}
