using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;
using System.IO;
using System.Globalization;
using System.Windows.Media.Imaging;

[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyExpirationDate("2025-12-31")]

// Expiration date attribute
public class AssemblyExpirationDate : Attribute
{
    private readonly string expirationDate;
    
    public string ExpirationDate 
    { 
        get { return expirationDate; } 
    }
    
    public AssemblyExpirationDate(string expirationDate)
    {
        this.expirationDate = expirationDate;
    }
}

// Simple License Verification
public static class SimpleLicenseVerifier
{
    private const string HARDCODED_ACCESS_CODE = "2b5d666c";
    private const string APP_SETTINGS_SUBDIR = "MAAS-ProtonSnoutCollision-Settings";

    private static string GetAppSettingsDir()
    {
        string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appSpecificDir = Path.Combine(appDataDir, APP_SETTINGS_SUBDIR);
        try
        {
            Directory.CreateDirectory(appSpecificDir); // Ensure it exists
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Failed to create directory {0} in AppData: {1}", APP_SETTINGS_SUBDIR, ex.Message));
        }
        return appSpecificDir;
    }

    private static string GetLicenseFilePath(string projectName)
    {
        return Path.Combine(GetAppSettingsDir(), projectName + "_license.txt");
    }

    public static string GetNoExpireFilePath()
    {
        return Path.Combine(GetAppSettingsDir(), "NOEXPIRE");
    }

    public static string GetNoAgreeFilePath()
    {
        return Path.Combine(GetAppSettingsDir(), "NoAgree.txt");
    }

    public static string GetValidatedFilePath(string projectName)
    {
        return Path.Combine(GetAppSettingsDir(), projectName + "_validated.txt");
    }
    
    public static bool IsLicenseAccepted(string projectName, string version)
    {
        string licenseFile = GetLicenseFilePath(projectName);
        if (!File.Exists(licenseFile))
            return false;
            
        try
        {
            string storedCode = File.ReadAllText(licenseFile).Trim();
            return VerifyAccessCode(storedCode);
        }
        catch
        {
            return false;
        }
    }
    
    public static bool ShowLicenseDialog(string projectName, string version, string licenseUrl)
    {
        // Create simple WPF window for license acceptance
        Window licenseWindow = new Window();
        licenseWindow.Title = "License Required"; 
        licenseWindow.Width = 450; // Slightly wider for longer instruction text
        licenseWindow.Height = 280; // Slightly taller
        licenseWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        licenseWindow.ResizeMode = ResizeMode.NoResize;
        licenseWindow.Background = Brushes.White;

        // Create layout
        StackPanel mainPanel = new StackPanel();
        mainPanel.Margin = new Thickness(20);

        // Title
        TextBlock title = new TextBlock();
        title.Text = "License Acceptance Required";
        title.FontSize = 16;
        title.FontWeight = FontWeights.Bold;
        title.Margin = new Thickness(0, 0, 0, 15);
        mainPanel.Children.Add(title);

        // Instructions
        TextBlock instructions = new TextBlock();
        instructions.Text = "Please visit the following URL to obtain your access code:\n" + licenseUrl + "\n\nEnter the access code below to continue.";
        instructions.TextWrapping = TextWrapping.Wrap;
        instructions.Margin = new Thickness(0, 0, 0, 20);
        mainPanel.Children.Add(instructions);
        
        // Access code input
        TextBlock codeLabel = new TextBlock();
        codeLabel.Text = "Access Code:";
        codeLabel.FontWeight = FontWeights.Bold;
        codeLabel.Margin = new Thickness(0, 0, 0, 5);
        mainPanel.Children.Add(codeLabel);

        TextBox codeTextBox = new TextBox();
        codeTextBox.Width = 200;
        codeTextBox.Height = 25;
        codeTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        codeTextBox.Margin = new Thickness(0, 0, 0, 20);
        mainPanel.Children.Add(codeTextBox);

        // Buttons
        StackPanel buttonPanel = new StackPanel();
        buttonPanel.Orientation = Orientation.Horizontal;
        buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;

        Button okButton = new Button();
        okButton.Content = "OK";
        okButton.Width = 80;
        okButton.Height = 25;
        okButton.Margin = new Thickness(0, 0, 10, 0);
        okButton.IsDefault = true;

        Button cancelButton = new Button();
        cancelButton.Content = "Cancel";
        cancelButton.Width = 80;
        cancelButton.Height = 25;
        cancelButton.IsCancel = true;

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        mainPanel.Children.Add(buttonPanel);

        licenseWindow.Content = mainPanel;

        bool result = false;

        okButton.Click += (sender, e) =>
        {
            string enteredCode = codeTextBox.Text.Trim();
            if (VerifyAccessCode(enteredCode))
            {
                try
                {
                    string licenseFile = GetLicenseFilePath(projectName);
                    File.WriteAllText(licenseFile, enteredCode);
                    result = true;
                    licenseWindow.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not save license acceptance: " + ex.Message, 
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Invalid access code. Please try again.", 
                              "Invalid Code", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };

        cancelButton.Click += (sender, e) =>
        {
            result = false;
            licenseWindow.Close();
        };

        // Set focus to text box
        licenseWindow.Loaded += (sender, e) => codeTextBox.Focus();

        licenseWindow.ShowDialog();
        return result;
    }
    
    private static bool VerifyAccessCode(string inputCode)
    {
        return string.Equals(inputCode, HARDCODED_ACCESS_CODE, StringComparison.OrdinalIgnoreCase);
    }
}

// USER NOTES:
// The SnoutPosition property requires version 16 or greater of Eclipse
// Comment out lines with this property to experiment with previous versions


namespace VMS.TPS
{
    //Snout
    public class Snout
    {
        public const double snout_pos_min = 0;
        public const double snout_pos_max = 421;
        //snout cover sizes for 3D model
        const double snout_face_zmin = -350;
        const double snout_face_zmax = 350;
        const double snout_face_xmin = -250;
        const double snout_face_xmax = 250;
        const double snout_end_zmin = -400;
        const double snout_end_zmax = 400;
        const double snout_end_xmin = -300;
        const double snout_end_xmax = 300;
        const double snout_depth = 100;

        private double current_snout_distance;
        private double planned_snout_distance;
        private Vector3D planned_snout_position;
        private double air_gap = Double.NaN;
        private Point3D air_gap_start;
        private Point3D air_gap_end;
        private double gantry_angle;
        private double couch_rotation;
        private Vector3D isocenter;
        private Model3DGroup snout_geometry;
        private MeshGeometry3D snout_mesh;
        private bool in_collision = false;
        private bool air_gap_not_found = true;

        public bool In_Collision
        {
            get { return in_collision; }
        }

        public bool Air_Gap_Not_Found
        {
            get { return air_gap_not_found; }
        }

        public double Snout_Distance
        {
            get { return current_snout_distance; }
        }

        public Vector3D Planned_Snout_Position
        {
            get { return planned_snout_position; }
        }

        public Model3DGroup Geometry
        {
            get { return snout_geometry; }
        }

        public double Air_Gap
        {
            get { return air_gap; }
        }

        public Point3D Air_Gap_Start
        {
            get { return air_gap_start; }
        }

        public Point3D Air_Gap_End
        {
            get { return air_gap_end; }
        }

        public Snout(double snout_distance, double gantry_angle, double couch_rtn, Vector3D isocenter)
        {
            this.current_snout_distance = snout_distance;
            this.planned_snout_distance = snout_distance;
            this.gantry_angle = gantry_angle;
            this.couch_rotation = couch_rtn;
            this.isocenter = isocenter;
            Create3DGeometry();
        }

        private void Create3DGeometry()
        {
            //snout face center with gantry at 0            
            Vector3D snout_center_pos = new Vector3D(0, -planned_snout_distance, 0);

            //Calculating corners of snout cover

            //front/proximal face
            Vector3D bottomleftfront = Vector3D.Add(snout_center_pos, new Vector3D(snout_face_xmin, 0, snout_face_zmin));
            Vector3D bottomrightfront = Vector3D.Add(snout_center_pos, new Vector3D(snout_face_xmax, 0, snout_face_zmin));
            Vector3D topleftfront = Vector3D.Add(snout_center_pos, new Vector3D(snout_face_xmin, 0, snout_face_zmax));
            Vector3D toprightfront = Vector3D.Add(snout_center_pos, new Vector3D(snout_face_xmax, 0, snout_face_zmax));
            //back/distal end, assuming conical shape, size increases distally
            Vector3D bottomleftback = Vector3D.Add(snout_center_pos, new Vector3D(snout_end_xmin, -snout_depth, snout_end_zmin));
            Vector3D bottomrightback = Vector3D.Add(snout_center_pos, new Vector3D(snout_end_xmax, -snout_depth, snout_end_zmin));
            Vector3D topleftback = Vector3D.Add(snout_center_pos, new Vector3D(snout_end_xmin, -snout_depth, snout_end_zmax));
            Vector3D toprightback = Vector3D.Add(snout_center_pos, new Vector3D(snout_end_xmax, -snout_depth, snout_end_zmax));

            //gantry and couch rotation
            Matrix3D matrix3D = Matrix3D.Identity;
            Quaternion gantry_rot = new Quaternion(new Vector3D(0, 0, 1), gantry_angle);
            matrix3D.Rotate(gantry_rot);
            Quaternion couch_rot = new Quaternion(new Vector3D(0, 1, 0), couch_rotation);
            matrix3D.Rotate(couch_rot);

            //Rotating snout (all its corners) by gantry angle
            bottomleftfront = matrix3D.Transform(bottomleftfront);
            bottomrightfront = matrix3D.Transform(bottomrightfront);
            topleftfront = matrix3D.Transform(topleftfront);
            toprightfront = matrix3D.Transform(toprightfront);
            //back/distal end, assuming conical shape, size increases distally
            bottomleftback = matrix3D.Transform(bottomleftback);
            bottomrightback = matrix3D.Transform(bottomrightback);
            topleftback = matrix3D.Transform(topleftback);
            toprightback = matrix3D.Transform(toprightback);

            planned_snout_position = matrix3D.Transform(snout_center_pos);

            //Shifting by isocenter:
            planned_snout_position = Vector3D.Add(planned_snout_position, isocenter);

            bottomleftfront = Vector3D.Add(bottomleftfront, isocenter);
            bottomrightfront = Vector3D.Add(bottomrightfront, isocenter);
            topleftfront = Vector3D.Add(topleftfront, isocenter);
            toprightfront = Vector3D.Add(toprightfront, isocenter);
            bottomleftback = Vector3D.Add(bottomleftback, isocenter);
            bottomrightback = Vector3D.Add(bottomrightback, isocenter);
            topleftback = Vector3D.Add(topleftback, isocenter);
            toprightback = Vector3D.Add(toprightback, isocenter);

            //defining snout mesh for rotated snout
            snout_geometry = new Model3DGroup();
            GeometryModel3D snout_model = new GeometryModel3D();
            snout_mesh = new MeshGeometry3D();
            Point3D Identity = new Point3D(0, 0, 0); //to convert vectors to points for position definitions
            snout_mesh.Positions = new Point3DCollection() { Point3D.Add(Identity, bottomleftfront), Point3D.Add(Identity, bottomrightfront), Point3D.Add(Identity, topleftfront), Point3D.Add(Identity, toprightfront),
             Point3D.Add(Identity, bottomleftback), Point3D.Add(Identity, bottomrightback), Point3D.Add(Identity, topleftback), Point3D.Add(Identity, toprightback)};
            //back traingles not includes, just a hollow cover
            snout_mesh.TriangleIndices = new Int32Collection() { 0, 2, 1, 1, 2, 3, 0, 5, 4, 0, 1, 5, 1, 7, 5, 1, 3, 7, 3, 6, 7, 3, 2, 6, 2, 4, 6, 2, 0, 4 };
            snout_model.Geometry = snout_mesh;
            DiffuseMaterial snout_material = new DiffuseMaterial();
            snout_material.Brush = new SolidColorBrush(Colors.LightGray);
            snout_model.Material = snout_material;
            //inner side of snout
            DiffuseMaterial snout_back_material = new DiffuseMaterial();
            snout_back_material.Brush = new SolidColorBrush(Colors.Red);
            snout_model.BackMaterial = snout_back_material;
            snout_geometry.Children.Add(snout_model);

            //creating frame
            Line3D front_bottom = new Line3D(Point3D.Add(Identity, bottomleftfront), Point3D.Add(Identity, bottomrightfront), Brushes.Black, 2);
            snout_geometry.Children.Add(front_bottom.GeometryModel3D);
            Line3D front_top = new Line3D(Point3D.Add(Identity, topleftfront), Point3D.Add(Identity, toprightfront), Brushes.Black, 2);
            snout_geometry.Children.Add(front_top.GeometryModel3D);
            Line3D front_right = new Line3D(Point3D.Add(Identity, bottomrightfront), Point3D.Add(Identity, toprightfront), Brushes.Black, 2);
            snout_geometry.Children.Add(front_right.GeometryModel3D);
            Line3D front_left = new Line3D(Point3D.Add(Identity, bottomleftfront), Point3D.Add(Identity, topleftfront), Brushes.Black, 2);
            snout_geometry.Children.Add(front_left.GeometryModel3D);
        }

        public void MoveTo(double new_snout_distance)
        {
            this.current_snout_distance = new_snout_distance;

            Vector3D snout_axis = Vector3D.Subtract(planned_snout_position, isocenter);

            snout_geometry.Transform = new TranslateTransform3D(Vector3D.Multiply(new_snout_distance / planned_snout_distance - 1, snout_axis));
        }

        public double CalculateAirGap(Structure body, double resolution_x, double resolution_y)
        {
            in_collision = false;
            air_gap_not_found = true;
            int x_counter = 0;
            air_gap = Double.MaxValue;
            VVector closest_collision_point = new VVector(0, 0, 0);
            VVector offset_at_snout = new VVector(0, 0, 0);

            //corner points of snout at planned snout distance
            Point3D snout_bottom_left = snout_mesh.Positions[0];
            Point3D snout_bottom_right = snout_mesh.Positions[1];
            Point3D snout_top_left = snout_mesh.Positions[2];

            //shift vectors to move along snout face
            Vector3D delta_x = Point3D.Subtract(snout_bottom_right, snout_bottom_left);
            Vector3D delta_y = Point3D.Subtract(snout_top_left, snout_bottom_left);

            //normalization to given resolution
            delta_x = Vector3D.Multiply(Vector3D.Divide(delta_x, delta_x.Length), resolution_x);
            delta_y = Vector3D.Multiply(Vector3D.Divide(delta_y, delta_y.Length), resolution_y);

            //vector perpenducular to snout face, its direction is towards patient
            Vector3D normal = Vector3D.CrossProduct(delta_y, delta_x);

            //normalization of snout face normal:
            Vector3D normal_unit = Vector3D.Divide(normal, normal.Length);
            normal = Vector3D.Multiply(normal_unit, current_snout_distance);
            Vector3D normal_planned = Vector3D.Multiply(normal_unit, planned_snout_distance);

            //bottom left corner point at current snout distance
            Point3D start_corner = Point3D.Add(snout_bottom_left, Vector3D.Subtract(normal_planned, normal));


            while (snout_face_xmin + x_counter * resolution_x <= snout_face_xmax)
            {
                int y_counter = 0;
                while (snout_face_zmin + y_counter * resolution_y <= snout_face_zmax)
                {
                    //start and end points for segment profile
                    Vector3D shift = Vector3D.Add(Vector3D.Multiply(delta_x, x_counter), Vector3D.Multiply(delta_y, y_counter));
                    Point3D start3D = Point3D.Add(start_corner, shift);
                    Point3D end3D = Point3D.Add(start3D, normal);

                    VVector start = new VVector(start3D.X, start3D.Y, start3D.Z);
                    VVector end = new VVector(end3D.X, end3D.Y, end3D.Z);

                    BitArray array = new BitArray(10 * (int)current_snout_distance);

                    SegmentProfile profile = body.GetSegmentProfile(start, end, array);

                    int collision_index = 0;
                    Boolean found = false;
                    foreach (SegmentProfilePoint p in profile)
                    {
                        if (p.Value == true)
                        {
                            found = true;
                            break;
                        }
                        collision_index++;
                    }

                    if (found)
                    {
                        air_gap_not_found = false;

                        if (collision_index == 0)
                        {
                            in_collision = true;
                            break;
                        }
                        else
                        {
                            VVector collision_point = new VVector(profile[collision_index].Position.x, profile[collision_index].Position.y, profile[collision_index].Position.z);

                            Double current_air_gap_value = (collision_point - start).Length;

                            if (Math.Abs(current_air_gap_value) < Math.Abs(air_gap))
                            {
                                air_gap = current_air_gap_value;

                                closest_collision_point = collision_point;
                                offset_at_snout = start;
                            }
                        }
                    }
                    else
                    {
                        //not found
                    }
                    y_counter++;
                }
                x_counter++;
            }

            air_gap_start = new Point3D(closest_collision_point.x, closest_collision_point.y, closest_collision_point.z);
            air_gap_end = new Point3D(offset_at_snout.x, offset_at_snout.y, offset_at_snout.z);

            return air_gap;
        }
    }

    //GUI class
    public class WNDContent : UserControl
    {
        const double snout_pos_min = 0;
        const double snout_pos_max = 421;

        private Canvas canvas;
        public ComboBox field;
        private PerspectiveCamera camera;
        private Model3DGroup model3D;
        private System.Windows.Controls.Label snout_position;
        private System.Windows.Controls.Label view_angle;
        private TextBox txt_x;
        private TextBox txt_y;
        private Slider sl_snout_position;
        private System.Windows.Controls.Label air_gap;
        private Button btn_calculate;
        private Snout snout;

        public ScriptContext context { get; set; }

        public WNDContent()
        {
            Grid main_grid = new Grid();

            // Add two rows: one for the banner, one for the rest
            main_grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Top banner
            main_grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Rest of UI

            // TOP WARNING BANNER
            var topBanner = new Label();
            topBanner.Content = "* * * NOT VALIDATED FOR CLINICAL USE * * *";
            topBanner.Background = new SolidColorBrush(Colors.PaleVioletRed);
            topBanner.Foreground = new SolidColorBrush(Colors.Black);
            topBanner.HorizontalContentAlignment = HorizontalAlignment.Center;
            topBanner.FontWeight = FontWeights.Bold;
            topBanner.Padding = new Thickness(0, 2, 0, 2);
            topBanner.FontSize = 14;
            topBanner.VerticalAlignment = VerticalAlignment.Top;
            topBanner.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(topBanner, 0);
            main_grid.Children.Add(topBanner);

            // Create a container for the rest of the UI
            Grid contentGrid = new Grid();
            Grid.SetRow(contentGrid, 1);
            main_grid.Children.Add(contentGrid);

            //Top left section:
            Border top_left = new Border();
            top_left.Width = 300;
            top_left.Height = 70;
            top_left.HorizontalAlignment = HorizontalAlignment.Left;
            top_left.VerticalAlignment = VerticalAlignment.Top;
            top_left.BorderThickness = new Thickness(1, 1, 2, 2);
            top_left.CornerRadius = new CornerRadius(3);
            top_left.BorderBrush = Brushes.Brown;
            top_left.Margin = new Thickness(5, 5, 0, 0);
            contentGrid.Children.Add(top_left);

            Grid top_left_grid = new Grid();

            System.Windows.Controls.Label lbl_patient = new System.Windows.Controls.Label();
            lbl_patient.Content = "Patient:";
            top_left_grid.Children.Add(lbl_patient);

            System.Windows.Controls.Label lbl_plan = new System.Windows.Controls.Label();
            lbl_plan.Content = "Plan:";
            lbl_plan.Margin = new Thickness(0, 15, 0, 0);
            top_left_grid.Children.Add(lbl_plan);

            System.Windows.Controls.Label patient = new System.Windows.Controls.Label();
            patient.Name = "patient";
            patient.Margin = new Thickness(50, 0, 0, 0);
            top_left_grid.Children.Add(patient);

            System.Windows.Controls.Label plan = new System.Windows.Controls.Label();
            plan.Name = "plan";
            plan.Margin = new Thickness(50, 15, 0, 0);
            top_left_grid.Children.Add(plan);

            System.Windows.Controls.Label lbl_field = new System.Windows.Controls.Label();
            lbl_field.Content = "Select field:";
            lbl_field.Margin = new Thickness(0, 40, 0, 0);
            top_left_grid.Children.Add(lbl_field);

            field = new ComboBox();
            field.Name = "fields";
            field.Height = 20;
            field.Width = 150;
            field.Margin = new Thickness(50, 40, 0, 0);
            field.SelectionChanged += Field_SelectionChanged;
            top_left_grid.Children.Add(field);

            top_left.Child = top_left_grid;

            //Top right section:
            Border top_right = new Border();
            top_right.Width = 300;
            top_right.Height = 70;
            top_right.HorizontalAlignment = HorizontalAlignment.Left;
            top_right.VerticalAlignment = VerticalAlignment.Top;
            top_right.BorderThickness = new Thickness(1, 1, 2, 2);
            top_right.CornerRadius = new CornerRadius(3);
            top_right.BorderBrush = Brushes.Brown;
            top_right.Margin = new Thickness(310, 5, 0, 0);
            contentGrid.Children.Add(top_right);

            Grid top_right_grid = new Grid();

            System.Windows.Controls.Label lbl_airgap = new System.Windows.Controls.Label();
            lbl_airgap.Content = "Measure air gap at resolution:";
            lbl_airgap.Margin = new Thickness(0, 15, 0, 0);
            top_right_grid.Children.Add(lbl_airgap);

            System.Windows.Controls.Label lbl_x = new System.Windows.Controls.Label();
            lbl_x.Content = "x[mm]:";
            lbl_x.Margin = new Thickness(165, 0, 0, 0);
            top_right_grid.Children.Add(lbl_x);

            System.Windows.Controls.Label lbl_y = new System.Windows.Controls.Label();
            lbl_y.Content = "y[mm]:";
            lbl_y.Margin = new Thickness(210, 0, 0, 0);
            top_right_grid.Children.Add(lbl_y);

            btn_calculate = new Button();
            btn_calculate.Content = "Calculate";
            btn_calculate.Width = 60;
            btn_calculate.Height = 20;
            btn_calculate.Margin = new Thickness(5, 42, 0, 0);
            btn_calculate.HorizontalAlignment = HorizontalAlignment.Left;
            btn_calculate.VerticalAlignment = VerticalAlignment.Top;
            btn_calculate.Click += Btn_calculate_Click;
            top_right_grid.Children.Add(btn_calculate);

            txt_x = new TextBox();
            txt_x.Width = 35;
            txt_x.Height = 20;
            txt_x.Margin = new Thickness(170, 20, 0, 0);
            txt_x.HorizontalAlignment = HorizontalAlignment.Left;
            txt_x.VerticalAlignment = VerticalAlignment.Top;
            txt_x.PreviewKeyDown += txt_PreviewKeyDown;
            txt_x.TextChanged += txt_TextChanged;
            txt_x.Text = "10";
            top_right_grid.Children.Add(txt_x);

            txt_y = new TextBox();
            txt_y.Width = 35;
            txt_y.Height = 20;
            txt_y.Margin = new Thickness(215, 20, 0, 0);
            txt_y.HorizontalAlignment = HorizontalAlignment.Left;
            txt_y.VerticalAlignment = VerticalAlignment.Top;
            txt_y.PreviewKeyDown += txt_PreviewKeyDown;
            txt_y.TextChanged += txt_TextChanged; ;
            txt_y.Text = "10";
            top_right_grid.Children.Add(txt_y);

            air_gap = new System.Windows.Controls.Label();
            air_gap.Content = "Calculated air gap =";
            air_gap.Margin = new Thickness(100, 40, 0, 0);
            top_right_grid.Children.Add(air_gap);

            top_right.Child = top_right_grid;

            //Canvas controls
            System.Windows.Controls.Label lbl_view_angle = new System.Windows.Controls.Label();
            lbl_view_angle.Content = "View angle[deg]: ";
            lbl_view_angle.Margin = new Thickness(5, 80, 0, 0);
            contentGrid.Children.Add(lbl_view_angle);

            view_angle = new System.Windows.Controls.Label();
            view_angle.Margin = new Thickness(95, 80, 0, 0);
            view_angle.Content = 0;
            contentGrid.Children.Add(view_angle);

            Slider sl_view_angle = new Slider();
            sl_view_angle.Width = 450;
            sl_view_angle.Margin = new Thickness(160, 85, 0, 0);
            sl_view_angle.HorizontalAlignment = HorizontalAlignment.Left;
            sl_view_angle.Minimum = -180;
            sl_view_angle.Maximum = 180;
            sl_view_angle.Value = 0;
            sl_view_angle.ValueChanged += sl_view_angle_ValueChanged;
            contentGrid.Children.Add(sl_view_angle);

            System.Windows.Controls.Label lbl_snout_position = new System.Windows.Controls.Label();
            lbl_snout_position.Content = "Snout position[cm]: ";
            lbl_snout_position.Margin = new Thickness(5, 100, 0, 0);
            contentGrid.Children.Add(lbl_snout_position);

            snout_position = new System.Windows.Controls.Label();
            snout_position.Name = "snout_position_value";
            snout_position.Margin = new Thickness(110, 100, 0, 0);
            contentGrid.Children.Add(snout_position);

            sl_snout_position = new Slider();
            sl_snout_position.Name = "snout_position";
            sl_snout_position.Width = 450;
            sl_snout_position.Margin = new Thickness(160, 105, 0, 0);
            sl_snout_position.HorizontalAlignment = HorizontalAlignment.Left;
            sl_snout_position.Minimum = snout_pos_min;
            sl_snout_position.Maximum = snout_pos_max;
            sl_snout_position.Value = 0;
            sl_snout_position.ValueChanged += sl_snout_position_ValueChanged;
            contentGrid.Children.Add(sl_snout_position);

            canvas = new Canvas();
            canvas.Margin = new Thickness(0, 130, 0, 0);
            canvas.Background = Brushes.LightSkyBlue;
            canvas.MouseWheel += Canvas_MouseWheel;
            contentGrid.Children.Add(canvas);



            Button btn_help = new Button();
            btn_help.Width = 20;
            btn_help.Height = 20;
            btn_help.Content = "?";
            btn_help.Click += Btn_help_Click;
            btn_help.Background = new SolidColorBrush(Color.FromArgb(128, 221, 221, 221));
            canvas.Children.Add(btn_help);
            Canvas.SetRight(btn_help, 0);

            // Create the TextBlock
            TextBlock myTextBlock = new TextBlock();

            // Set the row property of the TextBlock to 1
            Grid.SetRow(myTextBlock, 1);

            // Set the name and background of the TextBlock
            myTextBlock.Name = "Footer";
            myTextBlock.Background = new SolidColorBrush(Colors.PaleVioletRed);

            // Create the first Label with a Hyperlink
            var label1 = new Label();
            Hyperlink hyperlink = new Hyperlink();
            hyperlink.NavigateUri = new Uri("http://medicalaffairs.varian.com/download/VarianLUSLA.pdf");
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            hyperlink.Inlines.Add("Bound by the terms of the Varian LUSLA");
            label1.Content = hyperlink;
            label1.Margin = new Thickness(0);
            // Add the Labels to the TextBlock
            myTextBlock.Inlines.Add(label1);



            contentGrid.Children.Add(myTextBlock);
            myTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
            myTextBlock.HorizontalAlignment = HorizontalAlignment.Stretch;

            AddBottomBanner(myTextBlock);

            this.Content = main_grid;
        }

        private void AddBottomBanner(TextBlock tb)
        {
            var label2 = new Label();
            label2.Content = "* * * NOT VALIDATED FOR CLINICAL USE * * *";
            tb.Inlines.Add(label2);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Btn_help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This script visualizes a snout for proton machine. \r\n\r\n" +
                "It allows moving the snout and calculating air gap.\r\n\r\n" +
                "Air gap is calculated using ray tracing from the snout down to the body. Raytracing can be done at a custom resolution.\r\n\r\n" +
                "Note: small value may require a few seconds to calculate depending on snout size");
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt.Text.Length == 0)
            {
                (sender as TextBox).Background = Brushes.OrangeRed;
                btn_calculate.IsEnabled = false;
            }
            else
            {
                Double input;
                Boolean IsNumber = Double.TryParse(txt.Text, out input);
                if (IsNumber)
                {
                    if ((input > 0) && (input <= 50))
                    {
                        (sender as TextBox).Background = Brushes.White;
                        btn_calculate.IsEnabled = true;
                    }
                    else
                    {
                        (sender as TextBox).Background = Brushes.OrangeRed;
                        btn_calculate.IsEnabled = false;
                    }
                }
                else
                {
                    (sender as TextBox).Background = Brushes.OrangeRed;
                    btn_calculate.IsEnabled = false;
                }
            }
        }

        //Calculate airgap at the given offset
        private void Btn_calculate_Click(object sender, RoutedEventArgs e)
        {
            //Get reference to body outline
            IonPlanSetup active_proton_plan = context.IonPlanSetup;
            StructureSet sset = active_proton_plan.StructureSet;
            Structure body = sset.Structures.Where(s => s.DicomType == "EXTERNAL").First();

            Double resolution_x = Convert.ToDouble(txt_x.Text);
            Double resolution_y = Convert.ToDouble(txt_y.Text);

            double air_gap_value = snout.CalculateAirGap(body, resolution_x, resolution_y);

            //Erase previously calculated airgap:
            if (model3D.Children.Count == 6) //includes calculated gap which is the last child
            {
                model3D.Children.RemoveAt(5);
                air_gap.Content = "Calculated air gap = ";
            }

            if (snout.Air_Gap_Not_Found)
            {
                //in collision
                air_gap.Content = "Calculated air gap = NOT FOUND";
            }
            else
            {
                if (!snout.In_Collision)
                {
                    Line3D line = new Line3D(snout.Air_Gap_Start, snout.Air_Gap_End, Brushes.Red, 3);
                    model3D.Children.Add(line.GeometryModel3D);

                    air_gap.Content = "Calculated air gap = " + air_gap_value.ToString("###.0") + " mm";
                }
                else
                {
                    air_gap.Content = "Calculated air gap = IN COLLISION";
                }
            }

        }

        private void Field_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (field.SelectedIndex < 0) return; // Guard clause

            if (model3D != null)
            {
                Initiate3DView(); // This will re-initialize based on the new selection
            }
            
            air_gap.Content = "Calculated air gap = ";

            if (context.IonPlanSetup != null && context.IonPlanSetup.Beams.Any() && field.SelectedIndex < context.IonPlanSetup.Beams.Count())
            {
                IonBeam selectedBeam = context.IonPlanSetup.Beams.ElementAt(field.SelectedIndex) as IonBeam;
                if (selectedBeam != null)
                {
                    sl_snout_position.Value = selectedBeam.SnoutPosition * 10; //converting to mm
                }
                else
                {
                    sl_snout_position.Value = 0; // Or some default/error state
                }
            }
            else
            {
                 sl_snout_position.Value = 0; // Or some default/error state
            }
        }

        private void txt_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((e.Key >= System.Windows.Input.Key.D0) && (e.Key <= System.Windows.Input.Key.D9)) || ((e.Key >= System.Windows.Input.Key.NumPad0) && (e.Key <= System.Windows.Input.Key.NumPad9)) ||
                (e.Key == System.Windows.Input.Key.Back) || (e.Key == System.Windows.Input.Key.Delete))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        //to zoom in and out the 3D model with mouse wheel
        private void Canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Point3D point = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z);
            Matrix3D scalematrix = Matrix3D.Identity;
            if (e.Delta > 0)
            {
                scalematrix.Scale(new Vector3D(1.1, 1.1, 1.1));
            }
            else
            {
                scalematrix.Scale(new Vector3D(0.9, 0.9, 0.9));
            }
            point = Point3D.Multiply(point, scalematrix);
            camera.Position = point;
        }

        //to rotate view with slider
        private void sl_view_angle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            view_angle.Content = (sender as Slider).Value.ToString("000.0");

            if (model3D != null)
            {
                model3D.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), e.NewValue), 0, 0, 0);
            }
        }

        //to move the snout
        private void sl_snout_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;

            if (model3D != null)
            {
                //Erase previously calculated airgap:
                if (model3D.Children.Count == 6) //includes calculated gap which is the last child
                {
                    model3D.Children.RemoveAt(5);
                    air_gap.Content = "Calculated air gap = ";
                }

                snout_position.Content = (e.NewValue / 10).ToString("##.0");

                snout.MoveTo(e.NewValue);
            }
        }

        public void Initiate3DView()
        {
            if (context == null || context.IonPlanSetup == null || !context.IonPlanSetup.Beams.Any())
            {
                MessageBox.Show("No beams found in the current ion plan. Cannot initiate 3D view.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if(canvas.Children.Count > 1 && canvas.Children[1] is Viewport3D) canvas.Children.RemoveAt(1); 
                model3D = null; 
                return;
            }

            if (field.SelectedIndex < 0 || field.SelectedIndex >= context.IonPlanSetup.Beams.Count())
            {
                if(context.IonPlanSetup.Beams.Any()) field.SelectedIndex = 0; 
                else 
                {
                     MessageBox.Show("No beams available to select.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                     return;
                }
            }

            IonPlanSetup active_proton_plan = context.IonPlanSetup;
            IonBeam selectedBeam = active_proton_plan.Beams.ElementAt(field.SelectedIndex) as IonBeam;

            if (selectedBeam == null)
            {
                MessageBox.Show("Selected field is not a valid IonBeam.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (selectedBeam.ControlPoints == null || !selectedBeam.ControlPoints.Any())
            {
                MessageBox.Show("Selected beam has no control points.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StructureSet sset = active_proton_plan.StructureSet;
            Structure body = null;
            if (sset != null)
            {
                body = sset.Structures.FirstOrDefault(s => s.DicomType == "EXTERNAL");
            }
            
            if (body == null)
            {
                MessageBox.Show("EXTERNAL body structure not found.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            //creating visual model
            ModelVisual3D modelvisual = new ModelVisual3D();
            model3D = new Model3DGroup();
            modelvisual.Content = model3D;

            Viewport3D viewport = new Viewport3D();
            Canvas.SetLeft(viewport, 0);
            Canvas.SetTop(viewport, 0);
            if (canvas.Children.Count > 1 && canvas.Children[1] is Viewport3D) // Check if second child is Viewport3D
            {
                canvas.Children.RemoveAt(1);
            }
            else if (canvas.Children.Count == 1 && !(canvas.Children[0] is Button)) // If only one child and it's not the help button
            {
                 // This case might indicate an unexpected state, potentially clear all but button
            }

            canvas.Children.Add(viewport);
            // ... bindings ...
            Binding binding_w = new Binding();
            binding_w.Path = new PropertyPath(Canvas.ActualWidthProperty);
            binding_w.Source = canvas;
            viewport.SetBinding(Viewport3D.WidthProperty, binding_w);
            Binding binding_h = new Binding();
            binding_h.Path = new PropertyPath(Canvas.ActualHeightProperty);
            binding_h.Source = canvas;
            viewport.SetBinding(Viewport3D.HeightProperty, binding_h);
            viewport.Children.Add(modelvisual);

            VVector v_isocenter = selectedBeam.IsocenterPosition;
            Vector3D isocenter = new Vector3D(v_isocenter.x, v_isocenter.y, v_isocenter.z);

            GeometryModel3D patientmodel = new GeometryModel3D();
            patientmodel.Geometry = body.MeshGeometry;
            DiffuseMaterial dm = new DiffuseMaterial();
            dm.Brush = new SolidColorBrush(Color.FromScRgb(1, 0.8f, 0.7176470588235294f, 0.4784313725490196f));
            patientmodel.Material = dm;
            model3D.Children.Add(patientmodel);

            double gantry_angle = selectedBeam.ControlPoints[0].GantryAngle;
            double plan_snout_distance = selectedBeam.SnoutPosition * 10; 
            double couch_rtn = selectedBeam.ControlPoints[0].PatientSupportAngle;

            snout = new Snout(plan_snout_distance, gantry_angle, couch_rtn, isocenter);
            model3D.Children.Add(snout.Geometry);

            double snoutDistanceForAxis = snout.Snout_Distance != 0 ? snout.Snout_Distance : Snout.snout_pos_max; 
            Vector3D snout_parked = Vector3D.Multiply(Vector3D.Divide(Vector3D.Subtract(snout.Planned_Snout_Position, isocenter), snoutDistanceForAxis), Snout.snout_pos_max);
            Line3D CAX = new Line3D(Point3D.Add(new Point3D(0, 0, 0), Vector3D.Add(snout_parked, isocenter)), new Point3D(isocenter.X, isocenter.Y, isocenter.Z), Brushes.YellowGreen, 1);
            model3D.Children.Add(CAX.GeometryModel3D);

            DirectionalLight light1 = new DirectionalLight(Colors.WhiteSmoke, new Vector3D(0, 600, 0));
            model3D.Children.Add(light1);
            DirectionalLight light2 = new DirectionalLight(Colors.WhiteSmoke, new Vector3D(0, -600, 0));
            model3D.Children.Add(light2);

            camera = new PerspectiveCamera();
            camera.Position = new Point3D(0, -1500, 0);
            camera.LookDirection = new Vector3D(0, 1500, 0);
            camera.UpDirection = new Vector3D(0, 0, 1);
            viewport.Camera = camera;

            // Update UI elements that depend on the loaded field
            Label patientLabel = LogicalTreeHelper.FindLogicalNode(this, "patient") as Label;
            if (patientLabel != null) patientLabel.Content = context.Patient.FirstName + " " + context.Patient.LastName + " (ID:" + context.Patient.Id + ")";
            Label planLabel = LogicalTreeHelper.FindLogicalNode(this, "plan") as Label;
            if (planLabel != null) planLabel.Content = context.IonPlanSetup.Id;
            if (snout_position != null) snout_position.Content = (plan_snout_distance / 10).ToString("##.0");
            if (sl_snout_position != null) sl_snout_position.Value = plan_snout_distance; 
        }
    }

    //Class representing 3D line
    public class Line3D
    {
        private GeometryModel3D geoModel3D;
        private Point3D start;
        private Point3D end;

        Vector3D norm_to_line1;
        Vector3D norm_to_line2;

        public GeometryModel3D GeometryModel3D
        {
            get
            {
                return geoModel3D;
            }
        }

        private void GenerateVertices(Point3DCollection points)
        {
            points.Add(Point3D.Add(start, norm_to_line1));
            points.Add(Point3D.Add(start, norm_to_line2));
            points.Add(Point3D.Add(end, norm_to_line1));
            points.Add(Point3D.Add(end, norm_to_line2));
            points.Add(Point3D.Add(start, -norm_to_line1));
            points.Add(Point3D.Add(start, -norm_to_line2));
            points.Add(Point3D.Add(end, -norm_to_line1));
            points.Add(Point3D.Add(end, -norm_to_line2));
        }

        public Line3D(Point3D start, Point3D end, Brush brush, Double line_thickness)
        {
            this.end = end;
            this.start = start;

            Vector3D line = Point3D.Subtract(end, start);

            if ((Math.Abs(line.X) >= Math.Abs(line.Y)) && (Math.Abs(line.X) >= Math.Abs(line.Z)))
            {
                norm_to_line1 = new Vector3D(-(line.Y + line.Z) / line.X, 1, 1);
            }

            if ((Math.Abs(line.Y) >= Math.Abs(line.X)) && (Math.Abs(line.Y) >= Math.Abs(line.Z)))
            {
                norm_to_line1 = new Vector3D(1, -(line.X + line.Z) / line.Y, 1);
            }

            if ((Math.Abs(line.Z) >= Math.Abs(line.X)) && (Math.Abs(line.Z) >= Math.Abs(line.Y)))
            {
                norm_to_line1 = new Vector3D(1, 1, -(line.X + line.Y) / line.Z);
            }

            norm_to_line2 = Vector3D.CrossProduct(line, norm_to_line1);

            //Normalization:
            norm_to_line1 = Vector3D.Divide(norm_to_line1, norm_to_line1.Length);
            norm_to_line2 = Vector3D.Divide(norm_to_line2, norm_to_line2.Length);
            norm_to_line1 = Vector3D.Multiply(norm_to_line1, line_thickness);
            norm_to_line2 = Vector3D.Multiply(norm_to_line2, line_thickness);

            geoModel3D = new GeometryModel3D();
            DiffuseMaterial line_material = new DiffuseMaterial(brush);
            line_material.AmbientColor = Colors.Blue;
            geoModel3D.Material = line_material;

            MeshGeometry3D line_mesh = new MeshGeometry3D();
            GenerateVertices(line_mesh.Positions);
            line_mesh.TriangleIndices = new Int32Collection() { 0, 1, 2, 2, 1, 3, 5, 7, 4, 7, 6, 4, 1, 4, 6, 6, 3, 1, 5, 2, 7, 0, 2, 5, 7, 2, 3, 7, 3, 6, 4, 0, 5, 4, 1, 0 };
            geoModel3D.Geometry = line_mesh;
        }
    }

    // SnoutPreview wrapper class for the updated Script interface
    public class SnoutPreview : UserControl
    {
        private WNDContent content;
        
        public SnoutPreview(ScriptContext context, System.Windows.Window window, ScriptEnvironment environment, bool isValidated)
        {
            // Validate required context
            if (context.Patient == null)
            {
                throw new InvalidOperationException("No patient currently loaded.");
            }
            
            if (context.IonPlanSetup == null)
            {
                throw new InvalidOperationException("There are no proton plans opened. Please open a proton plan.");
            }
            
            // Create and configure the main content
            content = new WNDContent();
            content.context = context;
            
            // Set this control's content
            this.Content = content;

            // Populate fields ComboBox in WNDContent and set initial selection
            if (context.IonPlanSetup.IonBeams.Any())
            {
                foreach (IonBeam beam in context.IonPlanSetup.IonBeams)
                {
                    content.field.Items.Add(beam.Id); // Access 'field' through 'content'
                }
                content.field.SelectedIndex = 0; // Set selected index here
            }
            else
            {
                // Handle case with no beams, perhaps disable UI or show message in WNDContent
            }
            
            // Initialize the 3D view AFTER ComboBox is populated and index is set
            content.Initiate3DView();
        }
    }

    public class Script
    {
        private const string PROJECT_NAME = "ProtonSnoutCollision";
        private const string PROJECT_VERSION = "1.0.0";
        private const string LICENSE_URL = "https://varian-medicalaffairsappliedsolutions.github.io/MAAS-ProtonSnoutCollision";
        private const string GITHUB_URL = "https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-ProtonSnoutCollision";

        //USER MODIFIABLE: After validating this change below to true to remove NOT VALIDATED text in UI
        bool IsValidated = false;

        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context, System.Windows.Window window, ScriptEnvironment environment)
        {
            try
            {
                // Path variable no longer needed here as SimpleLicenseVerifier handles its paths
                
                // Check for NOEXPIRE file using AppData path
                bool bNoExpire = File.Exists(SimpleLicenseVerifier.GetNoExpireFilePath());
                
                // Check for NoAgree.txt file using AppData path
                bool skipAgree = File.Exists(SimpleLicenseVerifier.GetNoAgreeFilePath());

                // Simple license verification - RE-ENABLED (Simplified)
                if (!skipAgree && !SimpleLicenseVerifier.IsLicenseAccepted(PROJECT_NAME, PROJECT_VERSION))
                {
                    if (!SimpleLicenseVerifier.ShowLicenseDialog(PROJECT_NAME, PROJECT_VERSION, LICENSE_URL))
                    {
                        MessageBox.Show("License acceptance is required to use this application.", 
                                      "License Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Check expiration - RE-ENABLED
                var asmCa = typeof(Script).Assembly.CustomAttributes
                    .FirstOrDefault(ca => ca.AttributeType == typeof(AssemblyExpirationDate));
                
                DateTime endDate;
                if (asmCa != null &&
                    DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string,
                        new CultureInfo("en-US"), DateTimeStyles.None, out endDate))
                {
                    if (DateTime.Now > endDate && !bNoExpire)
                    {
                        MessageBox.Show("Application has expired. Newer builds with future expiration dates can be found here: " + GITHUB_URL);
                        return;
                    }
                
                    // Display opening msg - RE-ENABLED
                    string msg = "The current " + PROJECT_NAME + " application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
                    "application will only be available until " + endDate.ToShortDateString() + " after which the application will be unavailable. " +
                    "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support.\n\n" +
                    "Newer builds with future expiration dates can be found here: " + GITHUB_URL + "\n\n" +
                    "See the FAQ for more information on how to remove this pop-up and expiration";
                
                    string msg2 = "Application will only be available until " + endDate.ToShortDateString() + " after which the application will be unavailable. " +
                    "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support.\n\n" +
                    "Newer builds with future expiration dates can be found here: " + GITHUB_URL + "\n\n" +
                    "See the FAQ for more information on how to remove this pop-up and expiration";
                
                    // Check if validated (using simple file check from AppData) - RE-ENABLED
                    bool isValidatedUser = IsValidated; // Keep the class member for UI title for now
                    string validatedFilePath = SimpleLicenseVerifier.GetValidatedFilePath(PROJECT_NAME);
                    bool hasValidatedBefore = File.Exists(validatedFilePath);
                
                    if (!bNoExpire && !skipAgree)
                    {
                        if (!hasValidatedBefore)
                        {
                            // Show the first-time message
                            var res = MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo);
                
                            if (res == MessageBoxResult.No)
                            {
                                return;
                            }
                            
                            // Mark as validated for next time in AppData
                            try
                            {
                                File.WriteAllText(validatedFilePath, DateTime.Now.ToString());
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error saving validation status to AppData: " + ex.Message);
                                // Continue execution even if save fails
                            }
                            isValidatedUser = true; // Update for current session if agreed
                        }
                        else 
                        {
                            // Show the shorter message for subsequent uses
                            var res = MessageBox.Show(msg2, "Agreement", MessageBoxButton.YesNo);
                
                            if (res == MessageBoxResult.No)
                            {
                                return;
                            }
                            isValidatedUser = true; // User has seen agreement before and clicked Yes again
                        }
                    }
                    else
                    {
                        // If skipping agreement or no expiration, consider validated for UI purposes if `IsValidated` (class member) is true
                        // or if they had previously validated.
                        isValidatedUser = IsValidated || hasValidatedBefore;
                    }
                     // Update the IsValidated class member based on these checks for the UI title
                    IsValidated = isValidatedUser; 
                }

                // Continue with main application code
                if (context.Patient == null)
                {
                    MessageBox.Show("No patient currently loaded.");
                    return;
                }
                
                if (context.IonPlanSetup == null) 
                {
                    MessageBox.Show("No proton plan currently loaded.");
                    return;
                }

                var win = new SnoutPreview(context, window, environment, IsValidated);
                window.Content = win;
                window.Title = "MAAS - ProtonSnoutCollision" + (IsValidated ? "" : " NOT VALIDATED FOR CLINICAL USE");

                window.Width = 700; 
                window.Height = 850; 
                // window.SizeToContent = SizeToContent.WidthAndHeight; // Keep commented or set to Manual if using fixed size
                
                // Use the Loaded event to ensure the window is ready before activating
                RoutedEventHandler loadedEventHandler = null;
                loadedEventHandler = (s, e_loaded) => 
                {
                    window.WindowState = WindowState.Normal;
                    window.Activate();
                    // Remove the handler so it doesn't fire again if Loaded somehow re-triggers
                    window.Loaded -= loadedEventHandler;
                };
                window.Loaded += loadedEventHandler;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + (ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : ""));
            }
        }
    }
}