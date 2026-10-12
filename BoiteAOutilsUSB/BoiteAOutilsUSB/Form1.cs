using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoiteAOutilsUSB
{
    public partial class Form1 : Form
    {
        private Button btnTesterBatterie;
        private Label lblTitre;
        private Label lblFooter;

        // 1. Numéro de version actuel de ton application
        private const string CurrentVersion = "1.0.0";

        // 2. URLs directes vers ton dépôt GitHub Roroinformatique26/Projet-USB (branche main)
        private const string VersionUrl = "https://raw.githubusercontent.com/Roroinformatique26/Projet-USB/main/version.txt";
        private const string ExeUrl = "https://raw.githubusercontent.com/Roroinformatique26/Projet-USB/main/BoiteAOutilsUSB.exe";

        public Form1()
        {
            this.Text = "Boîte à Outils USB - Ri 26";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42); // Bleu nuit sombre du logo

            // Titre principal
            lblTitre = new Label();
            lblTitre.Text = "RI 26 - DIAGNOSTIC";
            lblTitre.ForeColor = Color.FromArgb(0, 229, 255); // Cyan lumineux du logo
            lblTitre.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitre.Left = 40;
            lblTitre.Top = 175;
            lblTitre.Width = 720;
            lblTitre.Height = 45;
            lblTitre.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitre);

            // Bouton "Tester la batterie"
            btnTesterBatterie = new Button();
            btnTesterBatterie.Text = "Lancer le test de la batterie";
            btnTesterBatterie.Left = 260;
            btnTesterBatterie.Top = 260;
            btnTesterBatterie.Width = 280;
            btnTesterBatterie.Height = 60;
            btnTesterBatterie.FlatStyle = FlatStyle.Flat;
            btnTesterBatterie.FlatAppearance.BorderColor = Color.FromArgb(0, 229, 255);
            btnTesterBatterie.FlatAppearance.BorderSize = 2;
            btnTesterBatterie.BackColor = Color.FromArgb(30, 41, 59);
            btnTesterBatterie.ForeColor = Color.White;
            btnTesterBatterie.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnTesterBatterie.Cursor = Cursors.Hand;

            btnTesterBatterie.Click += new EventHandler(BtnTesterBatterie_Click);
            this.Controls.Add(btnTesterBatterie);

            // Pied de page
            lblFooter = new Label();
            lblFooter.Text = "Ri 26 - Maintenance & Systèmes";
            lblFooter.ForeColor = Color.FromArgb(100, 116, 139);
            lblFooter.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblFooter.Left = 40;
            lblFooter.Top = 490;
            lblFooter.Width = 720;
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblFooter);

            // Lancement de la vérification des mises à jour dès que la fenêtre s'affiche
            this.Shown += async (sender, e) => await VerifierMiseAJourAsync();
        }

        private async Task VerifierMiseAJourAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Ri26-Updater");

                    // Télécharge la version distante depuis GitHub
                    string latestVersion = await client.GetStringAsync(VersionUrl);
                    latestVersion = latestVersion.Trim();

                    // Si la version est différente, on propose la mise à jour
                    if (latestVersion != CurrentVersion)
                    {
                        DialogResult result = MessageBox.Show(
                            $"Une nouvelle version ({latestVersion}) est disponible sur GitHub !\nVoulez-vous la télécharger et mettre à jour l'application ?",
                            "Ri 26 - Mise à jour disponible",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            await TelechargerEtAppliquerMiseAJourAsync(client);
                        }
                    }
                }
            }
            catch
            {
                // Si pas de connexion internet ou dépôt inaccessible, l'app continue normalement sans bloquer
            }
        }

        private async Task TelechargerEtAppliquerMiseAJourAsync(HttpClient client)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblFooter.Text = "Téléchargement de la mise à jour...";

                byte[] exeBytes = await client.GetByteArrayAsync(ExeUrl);

                string currentExePath = Process.GetCurrentProcess().MainModule?.FileName ?? "BoiteAOutilsUSB.exe";
                string tempExePath = Path.Combine(Path.GetTempPath(), "BoiteAOutilsUSB_new.exe");
                string batPath = Path.Combine(Path.GetTempPath(), "update_ri26.bat");

                // Enregistre le nouvel exécutable dans le dossier temporaire
                File.WriteAllBytes(tempExePath, exeBytes);

                // Script batch pour remplacer l'ancien exe et relancer l'app proprement
                string batchScript = $@"
                    @echo off
                    timeout /t 2 /nobreak > nul
                    move /y ""{tempExePath}"" ""{currentExePath}""
                    start """" ""{currentExePath}""
                    del ""{batPath}""
                ";

                File.WriteAllText(batPath, batchScript);

                // Exécute le script batch et ferme l'application actuelle
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = batPath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(psi);
                Application.Exit();
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("Erreur lors de la mise à jour : " + ex.Message, "Ri 26 - Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblFooter.Text = "Ri 26 - Maintenance & Systèmes";
            }
        }

        private void BtnTesterBatterie_Click(object? sender, EventArgs e)
        {
            try
            {
                string dossierAppli = AppDomain.CurrentDomain.BaseDirectory;
                string dossierRapports = Path.Combine(dossierAppli, "Rapports");

                if (!Directory.Exists(dossierRapports))
                {
                    Directory.CreateDirectory(dossierRapports);
                }

                string cheminFichierRapport = Path.Combine(dossierRapports, $"Rapport_Batterie_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                long designCapacity = 0;
                long fullChargeCapacity = 0;

                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM BatteryStaticData"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            designCapacity = Convert.ToInt64(obj["DesignedCapacity"]);
                            break;
                        }
                    }

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM BatteryFullChargedCapacity"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            fullChargeCapacity = Convert.ToInt64(obj["FullChargedCapacity"]);
                            break;
                        }
                    }
                }
                catch
                {
                    // Sécurité en cas de restriction
                }

                if (designCapacity > 0 && fullChargeCapacity > 0)
                {
                    double pourcentageSante = Math.Round(((double)fullChargeCapacity / designCapacity) * 100, 2);

                    string texte = $"--- RAPPORT DE SANTÉ DE LA BATTERIE (Ri 26) ---\n" +
                                   $"Date : {DateTime.Now}\n" +
                                   $"Capacité d'origine : {designCapacity} mWh\n" +
                                   $"Capacité actuelle : {fullChargeCapacity} mWh\n" +
                                   $"Santé de la batterie : {pourcentageSante} %";

                    File.WriteAllText(cheminFichierRapport, texte, System.Text.Encoding.UTF8);

                    MessageBox.Show($"Rapport de santé généré avec succès dans :\n{cheminFichierRapport}\n\nSanté : {pourcentageSante}%", "Ri 26 - Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Impossible de lire les données de la batterie. Assurez-vous d'être sur un ordinateur portable.", "Ri 26 - Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Ri 26 - Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}