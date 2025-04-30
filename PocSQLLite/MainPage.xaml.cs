using SiaqodbSyncProvider;

namespace PocSQLLite
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalDBService _dbService;
        private System.Guid _editHpId;

        public MainPage(LocalDBService dBService)
        {
            InitializeComponent();
            _dbService = dBService;
            Task.Run(async () => listView.ItemsSource = await _dbService.GetHps());
        }

        private async void saveButton_Clicked(object sender, EventArgs e)
        {
            if (_editHpId == System.Guid.Empty)
            {
                // Add Hp
                await _dbService.Create(new Hp
                {
                    TId = System.Guid.NewGuid(),
                    Name = nameEntryField.Text,
                    HpNr = hpnrEntryField.Text,
                    AISStatus = short.Parse(aisstatusEntryField.Text)
                });
            }
            else
            {
                // Update Hp
                await _dbService.Update(new Hp
                {
                    TId = _editHpId,
                    Name = nameEntryField.Text,
                    HpNr = hpnrEntryField.Text,
                    AISStatus = short.Parse(aisstatusEntryField.Text)
                });

                _editHpId = System.Guid.Empty;
            }

            nameEntryField.Text = string.Empty;
            hpnrEntryField.Text = string.Empty;
            aisstatusEntryField.Text = string.Empty;

            listView.ItemsSource = await _dbService.GetHps();
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var hp = (Hp)e.Item;
            var action = await DisplayActionSheet("Select Action", "Cancel", null, "Edit", "Delete");

            switch (action)
            {
                case "Edit":
                    _editHpId = hp.TId;
                    nameEntryField.Text = hp.Name;
                    hpnrEntryField.Text = hp.HpNr;
                    aisstatusEntryField.Text = hp.AISStatus.ToString();
                    break;
                case "Delete":
                    await _dbService.Delete(hp);
                    listView.ItemsSource = await _dbService.GetHps();
                    break;
            }
        }
    }
}
