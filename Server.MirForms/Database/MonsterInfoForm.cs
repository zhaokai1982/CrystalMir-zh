﻿using Server.MirDatabase;
using Server.MirEnvir;

namespace Server
{
    public partial class MonsterInfoForm : Form
    {
        public string MonsterListPath = Path.Combine(Settings.ExportPath, "MonsterList.txt");

        public Envir Envir => SMain.EditEnvir;

        private List<MonsterInfo> _selectedMonsterInfos;

        public MonsterInfoForm()
        {
            InitializeComponent();

            ImageComboBox.Items.AddRange(Enum.GetValues(typeof(Monster)).Cast<object>().ToArray());
            UpdateInterface();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            Envir.CreateMonsterInfo();
            UpdateInterface();
        }
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (_selectedMonsterInfos.Count == 0) return;

            if (MessageBox.Show("是否删除选定怪物信息", "删除怪物", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++) Envir.Remove(_selectedMonsterInfos[i]);

            if (Envir.MonsterInfoList.Count == 0) Envir.MonsterIndex = 0;

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            if (MonsterInfoListBox.Items.Count != Envir.MonsterInfoList.Count && string.IsNullOrWhiteSpace(TxtSearchMonster.Text.Trim()))
            {
                MonsterInfoListBox.Items.Clear();

                for (int i = 0; i < Envir.MonsterInfoList.Count; i++)
                    MonsterInfoListBox.Items.Add(Envir.MonsterInfoList[i]);
            }

            _selectedMonsterInfos = MonsterInfoListBox.SelectedItems.Cast<MonsterInfo>().ToList();

            if (_selectedMonsterInfos.Count == 0)
            {
                MonsterInfoPanel.Enabled = false;
                MonsterIndexTextBox.Text = string.Empty;
                MonsterNameTextBox.Text = string.Empty;

                ImageComboBox.SelectedItem = null;
                fileNameLabel.Text = "";
                AITextBox.Text = string.Empty;
                EffectTextBox.Text = string.Empty;
                LevelTextBox.Text = string.Empty;
                ViewRangeTextBox.Text = string.Empty;
                CoolEyeTextBox.Text = string.Empty;

                HPTextBox.Text = string.Empty;
                ExperienceTextBox.Text = string.Empty;

                MinACTextBox.Text = string.Empty;
                MaxACTextBox.Text = string.Empty;
                MinMACTextBox.Text = string.Empty;
                MaxMACTextBox.Text = string.Empty;
                MinDCTextBox.Text = string.Empty;
                MaxDCTextBox.Text = string.Empty;
                MinMCTextBox.Text = string.Empty;
                MaxMCTextBox.Text = string.Empty;
                MinSCTextBox.Text = string.Empty;
                MaxSCTextBox.Text = string.Empty;
                AccuracyTextBox.Text = string.Empty;
                AgilityTextBox.Text = string.Empty;
                LightTextBox.Text = string.Empty;

                ASpeedTextBox.Text = string.Empty;
                MSpeedTextBox.Text = string.Empty;

                CanPushCheckBox.Checked = false;
                CanTameCheckBox.Checked = false;
                AutoRevCheckBox.Checked = false;
                UndeadCheckBox.Checked = false;

                return;
            }

            MonsterInfo info = _selectedMonsterInfos[0];

            MonsterInfoPanel.Enabled = true;

            MonsterIndexTextBox.Text = info.Index.ToString();
            MonsterNameTextBox.Text = info.Name;
            DropPathTextBox.Text = info.DropPath;

            ImageComboBox.SelectedItem = null;
            ImageComboBox.SelectedItem = info.Image;
            fileNameLabel.Text = ((ushort)info.Image).ToString() + ".Lib";
            ushort imageValue = (ushort)info.Image;
            LoadImage(imageValue);

            AITextBox.Text = info.AI.ToString();
            EffectTextBox.Text = info.Effect.ToString();
            LevelTextBox.Text = info.Level.ToString();
            ViewRangeTextBox.Text = info.ViewRange.ToString();
            CoolEyeTextBox.Text = info.CoolEye.ToString();

            HPTextBox.Text = info.Stats[Stat.HP].ToString();
            ExperienceTextBox.Text = info.Experience.ToString();

            MinACTextBox.Text = info.Stats[Stat.最小防御].ToString();
            MaxACTextBox.Text = info.Stats[Stat.最大防御].ToString();
            MinMACTextBox.Text = info.Stats[Stat.最小魔御].ToString();
            MaxMACTextBox.Text = info.Stats[Stat.最大魔御].ToString();
            MinDCTextBox.Text = info.Stats[Stat.最小攻击].ToString();
            MaxDCTextBox.Text = info.Stats[Stat.最大攻击].ToString();
            MinMCTextBox.Text = info.Stats[Stat.最小魔法].ToString();
            MaxMCTextBox.Text = info.Stats[Stat.最大魔法].ToString();
            MinSCTextBox.Text = info.Stats[Stat.最小道术].ToString();
            MaxSCTextBox.Text = info.Stats[Stat.最大道术].ToString();
            AccuracyTextBox.Text = info.Stats[Stat.准确].ToString();
            AgilityTextBox.Text = info.Stats[Stat.敏捷].ToString();
            LightTextBox.Text = info.Light.ToString();

            ASpeedTextBox.Text = info.AttackSpeed.ToString();
            MSpeedTextBox.Text = info.MoveSpeed.ToString();


            CanPushCheckBox.Checked = info.CanPush;
            CanTameCheckBox.Checked = info.CanTame;
            AutoRevCheckBox.Checked = info.AutoRev;
            UndeadCheckBox.Checked = info.Undead;

            for (int i = 1; i < _selectedMonsterInfos.Count; i++)
            {
                info = _selectedMonsterInfos[i];

                if (MonsterIndexTextBox.Text != info.Index.ToString()) MonsterIndexTextBox.Text = string.Empty;
                if (MonsterNameTextBox.Text != info.Name) MonsterNameTextBox.Text = string.Empty;
                if (DropPathTextBox.Text != info.DropPath) DropPathTextBox.Text = string.Empty;

                if (ImageComboBox.SelectedItem == null || (Monster)ImageComboBox.SelectedItem != info.Image) ImageComboBox.SelectedItem = null;
                if (ImageComboBox.SelectedItem == null || (Monster)ImageComboBox.SelectedItem != info.Image) fileNameLabel.Text = "";
                if (AITextBox.Text != info.AI.ToString()) AITextBox.Text = string.Empty;
                if (EffectTextBox.Text != info.Effect.ToString()) EffectTextBox.Text = string.Empty;
                if (LevelTextBox.Text != info.Level.ToString()) LevelTextBox.Text = string.Empty;
                if (ViewRangeTextBox.Text != info.ViewRange.ToString()) ViewRangeTextBox.Text = string.Empty;
                if (CoolEyeTextBox.Text != info.CoolEye.ToString()) CoolEyeTextBox.Text = string.Empty;
                if (HPTextBox.Text != info.Stats[Stat.HP].ToString()) HPTextBox.Text = string.Empty;
                if (ExperienceTextBox.Text != info.Experience.ToString()) ExperienceTextBox.Text = string.Empty;

                if (MinACTextBox.Text != info.Stats[Stat.最小防御].ToString()) MinACTextBox.Text = string.Empty;
                if (MaxACTextBox.Text != info.Stats[Stat.最大防御].ToString()) MaxACTextBox.Text = string.Empty;
                if (MinMACTextBox.Text != info.Stats[Stat.最小魔御].ToString()) MinMACTextBox.Text = string.Empty;
                if (MaxMACTextBox.Text != info.Stats[Stat.最大魔御].ToString()) MaxMACTextBox.Text = string.Empty;
                if (MinDCTextBox.Text != info.Stats[Stat.最小攻击].ToString()) MinDCTextBox.Text = string.Empty;
                if (MaxDCTextBox.Text != info.Stats[Stat.最大攻击].ToString()) MaxDCTextBox.Text = string.Empty;
                if (MinMCTextBox.Text != info.Stats[Stat.最小魔法].ToString()) MinMCTextBox.Text = string.Empty;
                if (MaxMCTextBox.Text != info.Stats[Stat.最大魔法].ToString()) MaxMCTextBox.Text = string.Empty;
                if (MinSCTextBox.Text != info.Stats[Stat.最小道术].ToString()) MinSCTextBox.Text = string.Empty;
                if (MaxSCTextBox.Text != info.Stats[Stat.最大道术].ToString()) MaxSCTextBox.Text = string.Empty;
                if (AccuracyTextBox.Text != info.Stats[Stat.准确].ToString()) AccuracyTextBox.Text = string.Empty;
                if (AgilityTextBox.Text != info.Stats[Stat.敏捷].ToString()) AgilityTextBox.Text = string.Empty;
                if (LightTextBox.Text != info.Light.ToString()) LightTextBox.Text = string.Empty;
                if (ASpeedTextBox.Text != info.AttackSpeed.ToString()) ASpeedTextBox.Text = string.Empty;
                if (MSpeedTextBox.Text != info.MoveSpeed.ToString()) MSpeedTextBox.Text = string.Empty;

                if (CanPushCheckBox.Checked != info.CanPush) CanPushCheckBox.CheckState = CheckState.Indeterminate;
                if (CanTameCheckBox.Checked != info.CanTame) CanTameCheckBox.CheckState = CheckState.Indeterminate;

                if (AutoRevCheckBox.Checked != info.AutoRev) AutoRevCheckBox.CheckState = CheckState.Indeterminate;
                if (UndeadCheckBox.Checked != info.Undead) UndeadCheckBox.CheckState = CheckState.Indeterminate;
            }

        }
        private void RefreshMonsterList()
        {
            MonsterInfoListBox.SelectedIndexChanged -= MonsterInfoListBox_SelectedIndexChanged;

            List<bool> selected = new List<bool>();

            for (int i = 0; i < MonsterInfoListBox.Items.Count; i++) selected.Add(MonsterInfoListBox.GetSelected(i));
            MonsterInfoListBox.Items.Clear();
            for (int i = 0; i < Envir.MonsterInfoList.Count; i++) MonsterInfoListBox.Items.Add(Envir.MonsterInfoList[i]);
            for (int i = 0; i < selected.Count; i++) MonsterInfoListBox.SetSelected(i, selected[i]);

            MonsterInfoListBox.SelectedIndexChanged += MonsterInfoListBox_SelectedIndexChanged;
        }
        private void MonsterInfoListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInterface();
        }
        private void MonsterNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Name = ActiveControl.Text;

            RefreshMonsterList();
        }
        private void AITextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].AI = temp;
        }
        private void EffectTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Effect = temp;
        }
        private void LevelTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Level = temp;
        }
        private void LightTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Light = temp;
        }
        private void ViewRangeTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].ViewRange = temp;
        }
        private void HPTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            int temp;

            if (!int.TryParse(ActiveControl.Text, out temp) || temp < 0)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.HP] = temp;
        }
        private void ExperienceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            uint temp;

            if (!uint.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Experience = temp;
        }
        private void MinACTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最小防御] = temp;
        }
        private void MaxACTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最大防御] = temp;
        }
        private void MinMACTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最小魔御] = temp;
        }
        private void MaxMACTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最大魔御] = temp;
        }
        private void MinDCTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最小攻击] = temp;
        }
        private void MaxDCTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最大攻击] = temp;
        }
        private void MinMCTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最小魔法] = temp;
        }
        private void MaxMCTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最大魔法] = temp;
        }
        private void MinSCTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最小道术] = temp;
        }
        private void MaxSCTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp) || temp < 0 || temp > ushort.MaxValue)
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.最大道术] = temp;
        }
        private void AccuracyTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.准确] = temp;
        }
        private void AgilityTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Stats[Stat.敏捷] = temp;
        }
        private void ASpeedTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].AttackSpeed = temp;
        }
        private void MSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            ushort temp;

            if (!ushort.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].MoveSpeed = temp;
        }
        private void CanPushCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].CanPush = CanPushCheckBox.Checked;
        }
        private void CanTameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].CanTame = CanTameCheckBox.Checked;
        }
        private void AutoRevCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].AutoRev = AutoRevCheckBox.Checked;
        }

        private void UndeadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].Undead = UndeadCheckBox.Checked;
        }
        private void MonsterInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Envir.SaveDB();
        }

        private void PasteMButton_Click(object sender, EventArgs e)
        {
            string data = Clipboard.GetText();

            if (!data.StartsWith("Monster", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("无法粘贴，复制的数据不是怪物信息");
                return;
            }


            string[] monsters = data.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 1; i < monsters.Length; i++)
                MonsterInfo.FromText(monsters[i]);

            UpdateInterface();
        }

        private void CoolEyeTextBox_TextChanged(object sender, EventArgs e)
        {

            if (ActiveControl != sender) return;

            byte temp;

            if (!byte.TryParse(ActiveControl.Text, out temp))
            {
                ActiveControl.BackColor = Color.Red;
                return;
            }
            ActiveControl.BackColor = SystemColors.Window;


            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].CoolEye = temp;
        }

        private void ImageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
            {
                _selectedMonsterInfos[i].Image = (Monster)ImageComboBox.SelectedItem;
                fileNameLabel.Text = ((int)((Monster)ImageComboBox.SelectedItem)).ToString() + ".Lib";
            }
        }
        private void LoadImage(ushort imageValue)
        {
            string filename = $"{imageValue}.bmp";
            string imagePath = Path.Combine(Environment.CurrentDirectory, "Envir", "Previews", "Monsters", filename);

            if (File.Exists(imagePath))
            {
                using FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

                MonstersPreview.Image = Image.FromStream(fs);
            }
            else
            {
                MonstersPreview.Image = null;
            }
        }

        private void ExportAllButton_Click(object sender, EventArgs e)
        {
            ExportMonsters(Envir.MonsterInfoList);
        }

        private void ExportSelected_Click(object sender, EventArgs e)
        {
            var list = MonsterInfoListBox.SelectedItems.Cast<MonsterInfo>().ToList();

            ExportMonsters(list);
        }

        private void ExportMonsters(IEnumerable<MonsterInfo> monsters)
        {
            var monsterInfos = monsters as MonsterInfo[] ?? monsters.ToArray();
            var list = monsterInfos.Select(monster => monster.ToText()).ToList();

            File.WriteAllLines(MonsterListPath, list);

            MessageBox.Show(monsterInfos.Count() + " 信息已导出");
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            string Path = string.Empty;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text File|*.txt";
            ofd.ShowDialog();

            if (ofd.FileName == string.Empty) return;

            Path = ofd.FileName;

            string data;
            using (var sr = new StreamReader(Path))
            {
                data = sr.ReadToEnd();
            }

            var monsters = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var m in monsters)
                MonsterInfo.FromText(m);

            UpdateInterface();
        }

        private void DropBuilderButton_Click(object sender, EventArgs e)
        {
            MirForms.DropBuilder.DropGenForm GenForm = new MirForms.DropBuilder.DropGenForm();

            GenForm.ShowDialog();
        }

        private void DropPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ActiveControl != sender) return;

            var text = ActiveControl.Text;

            if (text.ToLower().EndsWith(".txt"))
            {
                text = text.Replace(".txt", "");
            }

            for (int i = 0; i < _selectedMonsterInfos.Count; i++)
                _selectedMonsterInfos[i].DropPath = text;
        }
        //private void TxtSearchMonster_TextChanged(object sender, EventArgs e)
        //{
        //    string searchText = TxtSearchMonster.Text.Trim();

        //    if (string.IsNullOrWhiteSpace(searchText))
        //    {
        //        ResetMonsterList();
        //        return;
        //    }

        //    MonsterInfo foundMonster = Envir.MonsterInfoList.FirstOrDefault(info => info.Name.Equals(searchText, StringComparison.OrdinalIgnoreCase));

        //    if (foundMonster != null)
        //    {
        //        MonsterInfoListBox.SelectedItem = foundMonster;
        //    }
        //}
        private void SearchForMonster()
        {
            MonsterInfoListBox.SelectedItem = "";
            List<MonsterInfo> results = Envir.MonsterInfoList.FindAll(x => x.Name.ToLower().Contains(TxtSearchMonster.Text.ToLower()));

            if (results.Count > 0)
            {
                MonsterInfoListBox.Items.Clear();
                foreach (MonsterInfo item in results)
                {
                    MonsterInfoListBox.Items.Add(item);
                }
            }
        }

        private void ResetMonsterList()
        {
            MonsterInfoListBox.Items.Clear();
            foreach (var monsterInfo in Envir.MonsterInfoList)
            {
                MonsterInfoListBox.Items.Add(monsterInfo);
            }
            TxtSearchMonster.Text = string.Empty;
        }

        private void TxtSearchMonster_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchForMonster();
            }
        }
    }
}