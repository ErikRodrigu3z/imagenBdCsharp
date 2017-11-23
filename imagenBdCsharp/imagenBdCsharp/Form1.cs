using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace imagenBdCsharp
{
    /*la tabla para la bd
     * 
     CREATE TABLE imagen(
	id_imagen int IDENTITY(1,1) NOT NULL,
	imagen image NOT NULL,
	nombre varchar(50) NULL     
     */

    public partial class Form1 : Form
    {
        //coection string
        private const string Conectar = @"Server=(local);Database=DatosClientes;User Id=sa;Password=Ads720510.;";
        SqlConnection cn = new SqlConnection(Conectar);
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            llenaGrid();
        }

        // boton buscar 
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            abrir.Filter = "*.bmp;*.gif;*.jpg;*.png|*.bmp;*.gif;*.jpg;*.png";
            abrir.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            abrir.Title = "Selecciona La Imagen Que Se Guardará En La Base De Datos";
            abrir.RestoreDirectory = true;

            if (abrir.ShowDialog() == DialogResult.OK)
            {
                label2.Text = abrir.FileName;
                txtNombre.Text = abrir.SafeFileName;

                pictureBox1.Image = Image.FromFile(abrir.FileName);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            }
            else
            {
                label2.Text = "";
                pictureBox1.Image = null;
            }
        }

        //metodo para insertar imagen 
        private void InsertarFotoBDD(int id, string fileFoto)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(fileFoto, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);

                ms.SetLength(fs.Length);
                fs.Read(ms.GetBuffer(), 0, (int)fs.Length);

                byte[] arrImg = ms.GetBuffer();
                ms.Flush();
                fs.Close();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cn.Open();

                   // cmd.CommandText = "insert into imagen values(@id_imagen,@nombre,@imagen)";
                    cmd.CommandText = "insert into imagen(nombre,imagen)values(@nombre,@imagen)";
                    cmd.Parameters.Add("@id_imagen", SqlDbType.BigInt).Value = id;
                    cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 50).Value = txtNombre.Text;
                    //cmd.Parameters.Add("@nombre", SqlDbType.NVarChar,50).Value = Path.GetDirectoryName(fileFoto);
                    cmd.Parameters.Add("@imagen", SqlDbType.VarBinary).Value = arrImg;

                    cmd.ExecuteNonQuery();
                    cn.Close();
                    MessageBox.Show("Imagen Guardada","Guardar Imagen",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                }

            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
 
        }

        //boton guardar imagen 
        private void button2_Click(object sender, EventArgs e)
        {
            string str = txtId.Text.Trim();
            int Num;
            bool isNum = int.TryParse(str, out Num);

            //if (!isNum)
            //{
            //    MessageBox.Show("Falta id Foto", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    textBox1.Focus();
            //}
            //else
            //{
                if (pictureBox1.Image == null || label2.Text == "")
                {
                    MessageBox.Show("Falta Seleccionar Foto", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    try
                    {
                        InsertarFotoBDD(Num, label2.Text);
                        llenaGrid();
                        txtNombre.ResetText();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            //}
        }

        //metodo para mostrar imagen 
        private Image ObtenerBitmapdeBDD(int id)
        {
           
            try
            {
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cn.Open();
                    
                    cmd.CommandText = "select imagen from imagen where id_imagen = " + id + "";
                    //cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = id;

                    byte[] arrImg = (byte[])cmd.ExecuteScalar();
                    cn.Close();
                    MemoryStream ms = new MemoryStream(arrImg);
                    Image img = Image.FromStream(ms);

                    ms.Close();

                    return img;
                }
            }
            catch (Exception e)
            {                
                throw new Exception(e.Message);
            }
        }

        //mostrar imagen de BD
        private void button3_Click(object sender, EventArgs e)
        {
            string str = txtId.Text.Trim();
            int Num;
            bool isNum = int.TryParse(str, out Num);

            if (!isNum)
            {
                MessageBox.Show("Falta id Foto", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtId.Focus();
            }           
            else
            {
                try
                {
                    label2.Text = "";
                    txtId.Text = "";
                    pictureBox1.Image = ObtenerBitmapdeBDD(Num);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }           
        }

        //metodo para llenar grid
        private void llenaGrid()
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                try
                {
                    cn.Open();
                    cmd.CommandText = "Select imagen,nombre from imagen";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                    cn.Close();                

                }
                catch (Exception e)
                {

                    throw new Exception(e.Message);
                }

            }
        }

        //evento click del grid
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])dataGridView1.CurrentRow.Cells[0].Value);
                pictureBox1.Image = Image.FromStream(ms);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }

        // boton copiar imagen 
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    // Add the selection to the clipboard.
                    Clipboard.SetImage(pictureBox1.Image);
                    MessageBox.Show("Imagen Copiada", "Copiar Imagen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        //boton guardar imagen a archivo
        private void button5_Click(object sender, EventArgs e)
        {
            //SaveFileDialog saveImg = new SaveFileDialog();
            //saveImg.Filter = "JPEG(*.JPG)|*.JPG|BMP(*.BMP)|*.BMP";
            //Image Imagen =  pictureBox1.BackgroundImage;
            //saveImg.ShowDialog();
            //Imagen.Save(saveImg.FileName);

            using (SaveFileDialog sfdlg = new SaveFileDialog())
            {
                sfdlg.Title = "Save Dialog";
                sfdlg.Filter = "JPEG(*.JPG)|*.JPG|BMP(*.BMP)|*.BMP|PNG(*.PNG)|*.PNG";

                if (sfdlg.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height))
                    {
                        pictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        // pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = pictureBox1.BackgroundImage;
                        //pictureBox1.Image.Save("c://cc.Jpg");
                        bmp.Save(sfdlg.FileName);
                        MessageBox.Show("Saved Successfully.....");

                    }
                }
            }
        }



    }//// end class
}
