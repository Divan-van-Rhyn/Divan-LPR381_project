using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace Test12
{
    public partial class frmLinearSolver : Form
    {
        Pen p = new Pen(Color.Black, 2);
        Pen p1 = new Pen(Color.Green, 2);
        Pen p2 = new Pen(Color.Red, 2);
        Pen p3 = new Pen(Color.Blue, 2);
        SolidBrush greenBrush = new SolidBrush(Color.FromArgb(64, 0, 150, 0));
        SolidBrush redBrush = new SolidBrush(Color.FromArgb(64, 255, 0, 0));
        System.Windows.Forms.Label[] AllDynamicLabels = new System.Windows.Forms.Label[50];

        public frmLinearSolver()
        {
            InitializeComponent();
            InitializeDataGridView(); 
            btnSaveTo.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void btnKnapsack_Click(object sender, EventArgs e)
        {
            if (!ValidateModel())
            {
                return;
            }

            try
            {
                var linearModel = ParseInput(redtInput.Text);
                var solution = SimplexSolver.Solve(linearModel);

                MessageBox.Show($"Optimal Value: {solution.OptimalValue}\n" +
                                $"Variable Values: {string.Join(", ", solution.VariableValues)}");

                // Clear previous DataGridView content
                dgvOptimal.Columns.Clear();
                dgvOptimal.Rows.Clear();

                int numColumns = linearModel.ObjectiveFunctionCoefficients.Count + linearModel.Constraints.Count + 1; // +1 for RHS
                //string[] columnNames = GetCustomColumnNames(numColumns);

                for (int i = 1; i < numColumns + 1; i++)
                {
                    dgvOptimal.Columns.Add($"Column{i}", "Column " + i);
                }

                // Populate the DataGridView with tableau data
                double[,] tableau = GetTableauData(linearModel);
                for (int i = 0; i < tableau.GetLength(0); i++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    for (int j = 0; j < tableau.GetLength(1); j++)
                    {
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = tableau[i, j] });
                    }
                    dgvOptimal.Rows.Add(row);
                }

                // Display optimal value permanently in a label
                lblOptimal.Text = $"Optimal Value = {solution.OptimalValue}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nExample format:\n\n" +
                                "max +1 +2 +3 +5 +6\n" +
                                "+1 +8 +6 +4 +1 <= 40\n" +
                                "+5 +5 +3 +9 +6 <= 30\n" +
                                "bin bin bin bin bin");
            }

            btnSaveTo.Enabled = dgvOptimal.Rows.Count > 0;
        }

        public Boolean ValidateModel()
        {
            if (redtInput.Lines.Count() < 3 || redtInput.Text.Count() == 0)
            {
                MessageBox.Show("Add a valid model before continuing!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            redtInput.Clear();
            dgvOptimal.Columns.Clear();
            dgvOptimal.Rows.Clear();
            lblOptimal.Text = "";
            btnSaveTo.Enabled = false;
        }

        private void btnTextfile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    redtInput.Text = File.ReadAllText(fileDialog.FileName);
                    btnSaveTo.Enabled = true;
                }
            }
        }

        private Model ParseInput(string input)
        {
            var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var objectiveFunction = new List<double>();
            var constraints = new List<Constraint>();
            var variableTypes = new List<VariableType>();
            bool isMaximization = true;

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts[0].ToLower() == "max" || parts[0].ToLower() == "min")
                {
                    isMaximization = parts[0].ToLower() == "max";
                    for (int i = 1; i < parts.Length; i++)
                    {
                        objectiveFunction.Add(double.Parse(parts[i]));
                    }
                }
                else if (parts.All(p => p == "bin" || p == "int" || p == "urs" || p == "+" || p == "-"))
                {
                    variableTypes.AddRange(parts.Select(ParseVariableType));
                }
                else
                {
                    var coefficients = new List<double>();
                    for (int i = 0; i < parts.Length - 2; i++)
                    {
                        coefficients.Add(double.Parse(parts[i]));
                    }

                    var constraint = new Constraint
                    {
                        Coefficients = coefficients,
                        RightHandSide = double.Parse(parts[parts.Length - 1])
                    };

                    if (coefficients.Count != objectiveFunction.Count)
                    {
                        throw new Exception("The number of coefficients in the constraints must match the number of decision variables.");
                    }

                    switch (parts[parts.Length - 2])
                    {
                        case "<=":
                            constraint.Operator = ConstraintOperator.LessThanOrEqual;
                            break;
                        case ">=":
                            constraint.Operator = ConstraintOperator.GreaterThanOrEqual;
                            break;
                        case "=":
                            constraint.Operator = ConstraintOperator.Equal;
                            break;
                        default:
                            throw new Exception("Invalid constraint operator");
                    }

                    constraints.Add(constraint);
                }
            }

            if (variableTypes.Count != objectiveFunction.Count)
            {
                throw new Exception("The number of variable types must match the number of decision variables.");
            }

            return new Model
            {
                ObjectiveFunctionCoefficients = objectiveFunction,
                Constraints = constraints,
                VariableTypes = variableTypes,
                IsMaximization = isMaximization
            };
        }


        private double[,] GetTableauData(Model model)
        {
            int numVariables = model.ObjectiveFunctionCoefficients.Count;
            int numConstraints = model.Constraints.Count;

            double[,] tableau = new double[numConstraints + 1, numVariables + numConstraints + 1];

            for (int i = 0; i < numVariables; i++)
            {
                tableau[0, i] = -model.ObjectiveFunctionCoefficients[i];
            }

            for (int i = 0; i < numConstraints; i++)
            {
                if (model.Constraints[i].Coefficients.Count != numVariables)
                {
                    throw new Exception($"Constraint {i} has {model.Constraints[i].Coefficients.Count} coefficients, expected {numVariables}.");
                }

                for (int j = 0; j < numVariables; j++)
                {
                    tableau[i + 1, j] = model.Constraints[i].Coefficients[j];
                }

                tableau[i + 1, numVariables + i] = 1;

                tableau[i + 1, numVariables + numConstraints] = model.Constraints[i].RightHandSide;
            }

            return tableau;
        }

        private void InitializeDataGridView()
        {
            // Initial setup for DataGridView if necessary
            dgvOptimal.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void lblOptimalValue_Click(object sender, EventArgs e)
        {

        }

        private VariableType ParseVariableType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Variable type cannot be null or empty.");
            }

            input = input.Trim();

            if (input == "bin")
            {
                return VariableType.Binary;
            }
            else if (input == "int")
            {
                return VariableType.Integer;
            }
            else if (input == "urs")
            {
                return VariableType.Unrestricted;
            }
            else if (input == "+")
            {
                return VariableType.NonNegative;
            }
            else if (input == "-")
            {
                return VariableType.NonPositive;
            }
            else
            {
                throw new Exception($"Invalid variable type '{input}'");
            }
        }

        private void lblVariableValues_Click(object sender, EventArgs e)
        {

        }

        private void lblErrorMessage_Click(object sender, EventArgs e)
        {

        }

        private void btnGraphical_Click(object sender, EventArgs e)
        {
            if (!ValidateModel())
            {
                return;
            }

            Graphics g = this.CreateGraphics();

            g.DrawLine(p, 335, 10, 335, 660);
            g.DrawLine(p, 10, 335, 660, 335);

            string[] lines = redtInput.Lines;
            int a = lines.Length;
            List<Double> coords = new List<double>();
            List<string> signs = new List<string>();
            double MaxVar = 0;
            for (int i = 1; lines.Length - 2 >= i; i++)//string manipulation
            {
                string controlString = lines[i];
                int test = controlString.IndexOf((char)32) + 1;
                double x1 = Convert.ToDouble(controlString.Substring(0, controlString.IndexOf((char)32)));
                controlString = controlString.Remove(0, controlString.IndexOf((char)32) + 1);
                double x2 = Convert.ToDouble(controlString.Substring(0, controlString.IndexOf((char)32)));
                controlString = controlString.Remove(0, controlString.IndexOf((char)32) + 1);
                string sign = controlString.Substring(0, controlString.IndexOf((char)32));
                signs.Add(sign);
                controlString = controlString.Remove(0, controlString.IndexOf((char)32) + 1);
                double rhs = Convert.ToDouble(controlString.Substring(0));

                if (rhs / x1 > rhs / x2 && rhs / x1 > MaxVar && x1 != 0) { MaxVar = rhs / x1; } //check Constraints 
                else if (rhs / x2 > MaxVar && x2 != 0) { MaxVar = rhs / x2; };
                if (x1 != 0)
                    coords.Add(rhs / x1);
                else
                    coords.Add(0);
                if (x2 != 0)
                    coords.Add(rhs / x2);
                else
                    coords.Add(0);
            }
            System.Windows.Forms.Label[] labels = new System.Windows.Forms.Label[coords.Count];
            double mulitipier = 325 / MaxVar;
            for (int i = 0; i < coords.Count; i += 2)//Lines desplayed on GUI    
            {
                int x1 = Convert.ToInt32(coords[i + 1] * mulitipier);
                int x2 = Convert.ToInt32(coords[i] * mulitipier);
                labels[i] = new System.Windows.Forms.Label();
                labels[i].Text = coords[i + 1].ToString();
                labels[i].Location = new Point(300, 340 - x1);
                labels[i].Size = new Size(25, 20);
                labels[i].BackColor = System.Drawing.Color.Transparent;
                labels[i].Name = "lbl" + i.ToString();
                this.Controls.Add(labels[i]);
                labels[i + 1] = new System.Windows.Forms.Label();
                labels[i + 1].Text = coords[i].ToString();
                labels[i + 1].Location = new Point(335 + x2, 340);
                labels[i + 1].Size = new Size(25, 20);
                labels[i + 1].Name = "lbl" + (i + 1).ToString();
                labels[i + 1].BackColor = System.Drawing.Color.Transparent;
                this.Controls.Add(labels[i + 1]);
                Pen tempPen = new Pen(Color.Yellow);
                if (signs[i / 2] == ">=")//picking Pen colors
                {
                    tempPen = p1;
                }
                else if (signs[i / 2] == "<=")
                {
                    tempPen = p2;
                }
                else if (signs[i / 2] == "=")
                {
                    tempPen = p3;
                }
                if (x1 == 0)
                {
                    g.DrawLine(tempPen, 335 + x2, 10, 335 + x2, 335);  //Feasible area
                    if (signs[i / 2] == "<=")
                    {
                        Point point1 = new Point(335 + x2, 10);
                        Point point2 = new Point(335, 10);
                        Point point3 = new Point(335, 335);
                        Point point4 = new Point(335 + x2, 335);
                        Point[] poligon = { point1, point2, point3, point4 };
                        g.FillPolygon(redBrush, poligon);

                    }
                    else if (signs[i / 2] == ">=")
                    {
                        Point point1 = new Point(335 + x2, 10);
                        Point point2 = new Point(660, 10);
                        Point point3 = new Point(660, 335);
                        Point point4 = new Point(335 + x2, 335);
                        Point[] poligon = { point1, point2, point3, point4 };
                        g.FillPolygon(greenBrush, poligon);
                    }
                }
                else if (x2 == 0)
                {
                    g.DrawLine(tempPen, 335, 335 - x1, 660, 335 - x1);
                    if (signs[i / 2] == "<=")
                    {
                        Point point1 = new Point(335, 335 - x1);
                        Point point2 = new Point(335, 335);
                        Point point3 = new Point(660, 335);
                        Point point4 = new Point(660, 335 - x1);
                        Point[] poligon = { point1, point2, point3, point4 };
                        g.FillPolygon(redBrush, poligon);

                    }
                    else if (signs[i / 2] == ">=")
                    {
                        Point point1 = new Point(335, 335 - x1);
                        Point point2 = new Point(335, 10);
                        Point point3 = new Point(660, 10);
                        Point point4 = new Point(660, 335 - x1);
                        Point[] poligon = { point1, point2, point3, point4 };
                        g.FillPolygon(greenBrush, poligon);
                    }
                }
                else
                {
                    g.DrawLine(tempPen, 335, 335 - x1, x2 + 335, 335);
                    if (signs[i / 2] == "<=")
                    {
                        Point point1 = new Point(335, 335 - x1);
                        Point point2 = new Point(x2 + 335, 335);
                        Point point3 = new Point(335, 335);
                        Point[] poligon = { point1, point2, point3 };
                        g.FillPolygon(redBrush, poligon);

                    }
                    else if (signs[i / 2] == ">=")
                    {
                        Point point1 = new Point(335, 335 - x1);
                        Point point2 = new Point(x2 + 335, 335);
                        Point point3 = new Point(660, 10);
                        Point point4 = new Point(335, 10);
                        Point[] poligon = { point1, point2, point3, point4 };
                        g.FillPolygon(greenBrush, poligon);
                    }
                }
            }
            AllDynamicLabels = labels;
        }

        private PointF GetIntersection(string constraint1, string constraint2)
        {
            // Parse coefficients and RHS
            var coeffs1 = ParseConstraint(constraint1);
            var coeffs2 = ParseConstraint(constraint2);

            double a1 = coeffs1.Item1;
            double b1 = coeffs1.Item2;
            double c1 = coeffs1.Item3;

            double a2 = coeffs2.Item1;
            double b2 = coeffs2.Item2;
            double c2 = coeffs2.Item3;

            // Calculate intersection point
            double det = a1 * b2 - a2 * b1;
            if (det == 0)
            {
                return new PointF(float.NaN, float.NaN); // No intersection
            }

            float x = (float)(b2 * c1 - b1 * c2) / (float)det;
            float y = (float)(a1 * c2 - a2 * c1) / (float)det;

            return new PointF(x, y);
        }

        // Method to check if the point is within bounds for plotting
        private bool IsPointInBounds(PointF point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X <= 325 && point.Y <= 325; // Adjust based on your scale
        }

        // Method to fill the feasible area
        private void FillFeasibleArea(Graphics g, List<PointF> intersectionPoints)
        {
            if (intersectionPoints.Count > 0)
            {
                // Sort points based on x coordinate
                var sortedPoints = intersectionPoints.OrderBy(p => p.X).ToList();

                // Create polygon points
                PointF[] polygon = sortedPoints.Select(p => new PointF(335 + p.X * (325 / 10), 340 - p.Y * (325 / 10))).ToArray(); // Adjust scaling

                // Fill the feasible area
                g.FillPolygon(greenBrush, polygon);
            }
        }
        // Method to parse a constraint string into coefficients
        private Tuple<double, double, double> ParseConstraint(string constraint)
        {
            string[] parts = constraint.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double x1 = Convert.ToDouble(parts[0]);
            double x2 = Convert.ToDouble(parts[1]);
            double rhs = Convert.ToDouble(parts[parts.Length - 1]);
            return Tuple.Create(x1, x2, rhs);
        }
        // Method to draw feasible areas based on the sign
        private void DrawFeasibleArea(Graphics g, string sign, int value, bool isVertical)
        {
            Point[] polygon;
            if (isVertical) // Handle vertical lines
            {
                if (sign == "<=")
                {
                    polygon = new Point[]
                    {
                new Point(335 + value, 10),
                new Point(335, 10),
                new Point(335, 660),
                new Point(335 + value, 660)
                    };
                    g.FillPolygon(greenBrush, polygon); // Fill with green for feasible area
                }
                else if (sign == ">=")
                {
                    polygon = new Point[]
                    {
                new Point(335 + value, 10),
                new Point(660, 10),
                new Point(660, 660),
                new Point(335 + value, 660)
                    };
                    g.FillPolygon(greenBrush, polygon); // Fill with green for feasible area
                }
            }
            else // Handle horizontal lines
            {
                if (sign == "<=")
                {
                    polygon = new Point[]
                    {
                new Point(335, 340),
                new Point(335, 340 - value),
                new Point(660, 340 - value),
                new Point(660, 340)
                    };
                    g.FillPolygon(greenBrush, polygon); // Fill with green for feasible area
                }
                else if (sign == ">=")
                {
                    polygon = new Point[]
                    {
                new Point(335, 340),
                new Point(335, 10),
                new Point(660, 10),
                new Point(660, 340)
                    };
                    g.FillPolygon(greenBrush, polygon); // Fill with green for feasible area
                }
            }

        }

        public class Model
        {
            public List<double> ObjectiveFunctionCoefficients { get; set; }
            public List<Constraint> Constraints { get; set; }
            public List<VariableType> VariableTypes { get; set; }
            public bool IsMaximization { get; set; }
        }

        public class Constraint
        {
            public List<double> Coefficients { get; set; }
            public double RightHandSide { get; set; }
            public ConstraintOperator Operator { get; set; }
        }

        public enum ConstraintOperator
        {
            LessThanOrEqual,
            GreaterThanOrEqual,
            Equal
        }

        public class Solution
        {
            public double OptimalValue { get; set; }
            public List<double> VariableValues { get; set; }
        }

        public enum VariableType
        {
            Binary,
            Integer,
            Unrestricted,
            NonNegative,
            NonPositive
        }

        public static class SimplexSolver
        {
            public static Solution Solve(Model model)
            {
                // Initialize the tableau
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
                    // Find the pivot column (most negative value in the objective row)
                    int pivotCol = FindPivotColumn(tableau);
                    if (pivotCol == -1) break; // Optimal solution found

                    // Find the pivot row (minimum ratio test)
                    int pivotRow = FindPivotRow(tableau, pivotCol);
                    if (pivotRow == -1) throw new Exception("Problem is unbounded.");

                    // Perform pivot operation
                    Pivot(tableau, pivotRow, pivotCol);
                }

                // Extract the solution
                return ExtractSolution(tableau, model);
            }

            private static int FindPivotColumn(double[,] tableau)
            {
                // Return the index of the most negative coefficient in the objective function row
                int pivotCol = -1;
                double minValue = 0;
                for (int j = 0; j < tableau.GetLength(1) - 1; j++) // Exclude the last column (RHS)
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
                for (int i = 1; i < tableau.GetLength(0); i++) // Exclude the objective function row
                {
                    if (tableau[i, pivotCol] > 0) // Only consider positive entries
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
                // Scale the pivot row
                double pivotValue = tableau[pivotRow, pivotCol];
                for (int j = 0; j < tableau.GetLength(1); j++)
                {
                    tableau[pivotRow, j] /= pivotValue; // Normalize the pivot row
                }

                // Eliminate the pivot column entries in other rows
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
                    solution.VariableValues[i] = tableau[i + 1, tableau.GetLength(1) - 1]; // Get values from the last column
                }

                return solution;
            }
        }

        private void lblOptimalValue_Click_1(object sender, EventArgs e)
        {

        }

        private void btnSimplex_Click(object sender, EventArgs e)
        {
            if (!ValidateModel())
            {
                return;
            }

            try
            {
                var model = ParseInput(redtInput.Text);
                var solution = SimplexSolverWithPivotDetails.Solve(model, out List<(int PivotRow, int PivotColumn)> pivotDetails);

                // Clear previous DataGridView content
                dgvOptimal.Columns.Clear();
                dgvOptimal.Rows.Clear();

                int numColumns = model.ObjectiveFunctionCoefficients.Count + model.Constraints.Count + 1; // +1 for RHS

                for (int i = 1; i < numColumns + 1; i++)
                {
                    dgvOptimal.Columns.Add($"Column{i}", "Column " + i);
                }

                // Populate the DataGridView with tableau data and mark the pivot rows/columns
                double[,] tableau = SimplexSolverWithPivotDetails.GetFinalTableau(model);
                for (int i = 0; i < tableau.GetLength(0); i++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    for (int j = 0; j < tableau.GetLength(1); j++)
                    {
                        var cell = new DataGridViewTextBoxCell() { Value = tableau[i, j] };

                        // Highlight the pivot row and column
                        if (pivotDetails.Any(p => p.PivotRow == i && p.PivotColumn == j))
                        {
                            cell.Style.BackColor = Color.Yellow;
                        }

                        row.Cells.Add(cell);
                    }
                    dgvOptimal.Rows.Add(row);
                }

                // Display optimal value permanently in a label
                lblOptimal.Text = $"Optimal Value: {solution.OptimalValue}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nExample format:\n\n" +
                                "max +60 +30 +20\n" +
                                "+8 +6 +1 <= 48\n" +
                                "+4 +2 +1.5 <= 20\n" +
                                "+2 +1.5 +0.5 <= 8\n" +
                                "int int int");
            }
            btnSaveTo.Enabled = dgvOptimal.Rows.Count > 0;
        }

        public static class SimplexSolverWithPivotDetails
        {
            public static Solution Solve(Model model, out List<(int PivotRow, int PivotColumn)> pivotDetails)
            {
                pivotDetails = new List<(int PivotRow, int PivotColumn)>();
                int numVariables = model.ObjectiveFunctionCoefficients.Count;
                int numConstraints = model.Constraints.Count;
                double[,] tableau = InitializeTableau(model);

                while (true)
                {
                    int pivotCol = FindPivotColumn(tableau);
                    if (pivotCol == -1) break;

                    int pivotRow = FindPivotRow(tableau, pivotCol);
                    if (pivotRow == -1) throw new Exception("Problem is unbounded.");

                    pivotDetails.Add((pivotRow, pivotCol));
                    Pivot(tableau, pivotRow, pivotCol);
                }

                return ExtractSolution(tableau, model);
            }

            private static double[,] InitializeTableau(Model model)
            {
                int numVariables = model.ObjectiveFunctionCoefficients.Count;
                int numConstraints = model.Constraints.Count;
                int numSlackSurplusVars = 0;
                int numArtificialVars = 0;

                foreach (var constraint in model.Constraints)
                {
                    if (constraint.Operator == ConstraintOperator.LessThanOrEqual)
                    {
                        numSlackSurplusVars++;
                    }
                    else if (constraint.Operator == ConstraintOperator.GreaterThanOrEqual)
                    {
                        numSlackSurplusVars++;
                        numArtificialVars++;
                    }
                    else if (constraint.Operator == ConstraintOperator.Equal)
                    {
                        numArtificialVars++;
                    }
                }

                double[,] tableau = new double[numConstraints + 1, numVariables + numSlackSurplusVars + numArtificialVars + 1];

                for (int i = 0; i < numVariables; i++)
                {
                    tableau[0, i] = model.IsMaximization ? -model.ObjectiveFunctionCoefficients[i] : model.ObjectiveFunctionCoefficients[i];
                }

                int slackSurplusIndex = numVariables;
                int artificialIndex = numVariables + numSlackSurplusVars;

                for (int i = 0; i < numConstraints; i++)
                {
                    for (int j = 0; j < numVariables; j++)
                    {
                        tableau[i + 1, j] = model.Constraints[i].Coefficients[j];
                    }

                    if (model.Constraints[i].Operator == ConstraintOperator.LessThanOrEqual)
                    {
                        tableau[i + 1, slackSurplusIndex++] = 1; // Slack variable
                    }
                    else if (model.Constraints[i].Operator == ConstraintOperator.GreaterThanOrEqual)
                    {
                        tableau[i + 1, slackSurplusIndex++] = -1; // Surplus variable
                        tableau[i + 1, artificialIndex++] = 1; // Artificial variable
                    }
                    else if (model.Constraints[i].Operator == ConstraintOperator.Equal)
                    {
                        tableau[i + 1, artificialIndex++] = 1; // Artificial variable
                    }

                    tableau[i + 1, tableau.GetLength(1) - 1] = model.Constraints[i].RightHandSide;
                }

                return tableau;
            }


            public static double[,] GetFinalTableau(Model model)
            {
                int numVariables = model.ObjectiveFunctionCoefficients.Count;
                int numConstraints = model.Constraints.Count;
                double[,] tableau = InitializeTableau(model);

                while (true)
                {
                    int pivotCol = FindPivotColumn(tableau);
                    if (pivotCol == -1) break;

                    int pivotRow = FindPivotRow(tableau, pivotCol);
                    if (pivotRow == -1) throw new Exception("Problem is unbounded.");

                    Pivot(tableau, pivotRow, pivotCol);
                }

                return tableau;
            }

            private static int FindPivotColumn(double[,] tableau)
            {
                int pivotCol = -1;
                double minValue = 0;
                for (int j = 0; j < tableau.GetLength(1) - 1; j++)
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
                for (int i = 1; i < tableau.GetLength(0); i++)
                {
                    if (tableau[i, pivotCol] > 0)
                    {
                        double ratio = tableau[i, tableau.GetLength(1) - 1] / tableau[i, pivotCol];
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
                    tableau[pivotRow, j] /= pivotValue;
                }

                for (int i = 0; i < tableau.GetLength(0); i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < tableau.GetLength(1); j++)
                        {
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                        }
                    }
                }
            }

            private static Solution ExtractSolution(double[,] tableau, Model model)
            {
                var solution = new Solution
                {
                    VariableValues = new List<double>(new double[model.ObjectiveFunctionCoefficients.Count])
                };

                solution.OptimalValue = -tableau[0, tableau.GetLength(1) - 1];

                for (int i = 0; i < model.ObjectiveFunctionCoefficients.Count; i++)
                {
                    solution.VariableValues[i] = tableau[i + 1, tableau.GetLength(1) - 1];
                }

                return solution;
            }
        }

        private void btnSaveTo_Click(object sender, EventArgs e)
        {
            if (dgvOptimal.Rows.Count == 0)
            {
                MessageBox.Show("No data to save. Please populate the DataGridView first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(lblOptimal.Text))
            {
                MessageBox.Show("Optimal value is missing. Please solve the problem first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Write the input values from redtInput
                            writer.WriteLine("Input Values:");
                            writer.WriteLine(redtInput.Text.Trim());
                            writer.WriteLine();

                            // Write the optimal value
                            writer.WriteLine("Optimal Value:");
                            writer.WriteLine(lblOptimal.Text);
                            writer.WriteLine();

                            // Determine the maximum width for each column
                            int[] columnWidths = new int[dgvOptimal.Columns.Count];
                            for (int i = 0; i < dgvOptimal.Columns.Count; i++)
                            {
                                columnWidths[i] = dgvOptimal.Columns[i].HeaderText.Length;
                            }

                            foreach (DataGridViewRow row in dgvOptimal.Rows)
                            {
                                for (int i = 0; i < row.Cells.Count; i++)
                                {
                                    int cellLength = row.Cells[i].Value?.ToString()?.Length ?? 0;
                                    if (cellLength > columnWidths[i])
                                    {
                                        columnWidths[i] = cellLength;
                                    }
                                }
                            }

                            // Write DataGridView column headers with appropriate spacing
                            writer.Write("".PadRight(columnWidths[0]));
                            for (int i = 0; i < dgvOptimal.Columns.Count; i++)
                            {
                                writer.Write(dgvOptimal.Columns[i].HeaderText.PadRight(columnWidths[i] + 2));
                            }
                            writer.WriteLine();

                            // Write DataGridView rows with appropriate spacing
                            foreach (DataGridViewRow row in dgvOptimal.Rows)
                            {
                                for (int i = 0; i < row.Cells.Count; i++)
                                {
                                    string cellValue = row.Cells[i].Value?.ToString() ?? "";
                                    writer.Write(cellValue.PadRight(columnWidths[i] + 2));
                                }
                                writer.WriteLine();
                            }
                        }

                        MessageBox.Show("Data saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


