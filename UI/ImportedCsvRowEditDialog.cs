using otomasyon.Localization;
using otomasyon.Models.Recipe;

namespace otomasyon.UI;

public sealed class ImportedCsvRowEditDialog : Form, ILocalizable
{
    private readonly CsvRowEditModel _model;
    private readonly Label _lblHint = new();
    private readonly Label _lblKalinlik = new();
    private readonly NumericUpDown _numKalinlik = new();
    private readonly Label _lblAdet = new();
    private readonly NumericUpDown _numAdet = new();
    private readonly FlowLayoutPanel _edgeFlow = new();
    private readonly FlowLayoutPanel _ventFlow = new();
    private readonly Dictionary<int, NumericUpDown> _edgeSaInputs = new();
    private readonly Dictionary<int, NumericUpDown> _edgeOffsetInputs = new();
    private readonly Dictionary<int, NumericUpDown> _ventSaInputs = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    public ImportedCsvRowEditDialog(CsvRowEditModel model)
    {
        _model = model;

        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 560);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var inner = new Panel { Dock = DockStyle.Top, AutoSize = true, Width = 440, Padding = new Padding(12) };

        _lblHint.AutoSize = false;
        _lblHint.Size = new Size(420, 36);
        _lblHint.ForeColor = Color.FromArgb(90, 90, 90);
        _lblHint.Location = new Point(0, 0);
        inner.Controls.Add(_lblHint);

        var lblKalinlik = _lblKalinlik;
        lblKalinlik.Location = new Point(0, 44);
        lblKalinlik.AutoSize = true;
        _numKalinlik.Location = new Point(200, 40);
        _numKalinlik.Width = 120;
        _numKalinlik.DecimalPlaces = 2;
        _numKalinlik.Minimum = 1;
        _numKalinlik.Maximum = 99999;
        _numKalinlik.Value = (decimal)Math.Max(1, model.KalinlikMm);
        inner.Controls.Add(lblKalinlik);
        inner.Controls.Add(_numKalinlik);

        var lblAdet = _lblAdet;
        lblAdet.Location = new Point(0, 78);
        lblAdet.AutoSize = true;
        _numAdet.Location = new Point(200, 74);
        _numAdet.Width = 120;
        _numAdet.Minimum = 1;
        _numAdet.Maximum = 99999;
        _numAdet.Value = model.Adet;
        inner.Controls.Add(lblAdet);
        inner.Controls.Add(_numAdet);

        int top = 112;
        if (model.ActiveEdgeIndices.Count > 0)
        {
            var lblEdges = new Label
            {
                Location = new Point(0, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            lblEdges.Text = L.Get("Setup.EdgesHint");
            inner.Controls.Add(lblEdges);
            top += 28;

            var edgeHeader = new Panel { Location = new Point(0, top), Width = 400, Height = 22 };
            edgeHeader.Controls.Add(new Label
            {
                Location = new Point(0, 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Text = L.Get("Setup.ColEdge")
            });
            edgeHeader.Controls.Add(new Label
            {
                Location = new Point(150, 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Text = L.Get("Setup.ColThickness")
            });
            edgeHeader.Controls.Add(new Label
            {
                Location = new Point(280, 4),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Text = L.Get("Setup.ColOffset")
            });
            inner.Controls.Add(edgeHeader);
            top += 26;

            _edgeFlow.FlowDirection = FlowDirection.TopDown;
            _edgeFlow.AutoSize = true;
            _edgeFlow.WrapContents = false;
            _edgeFlow.Location = new Point(0, top);
            _edgeFlow.Width = 420;

            foreach (int edgeIndex in model.ActiveEdgeIndices)
            {
                var row = new Panel { Width = 400, Height = 36 };
                row.Controls.Add(new Label
                {
                    Location = new Point(0, 8),
                    AutoSize = true,
                    Text = L.F("Setup.CsvEdgeLabel", edgeIndex)
                });

                var numSa = new NumericUpDown
                {
                    Location = new Point(150, 4),
                    Width = 110,
                    DecimalPlaces = 2,
                    Maximum = 99999,
                    Minimum = 0,
                    Value = (decimal)(model.SaByEdge.TryGetValue(edgeIndex, out double sa) ? sa : 0)
                };
                _edgeSaInputs[edgeIndex] = numSa;
                row.Controls.Add(numSa);

                var numOffset = new NumericUpDown
                {
                    Location = new Point(280, 4),
                    Width = 110,
                    DecimalPlaces = 2,
                    Maximum = 99999,
                    Minimum = 0,
                    Value = (decimal)(model.OffsetByEdge.TryGetValue(edgeIndex, out double offset) ? offset : 0)
                };
                _edgeOffsetInputs[edgeIndex] = numOffset;
                row.Controls.Add(numOffset);
                _edgeFlow.Controls.Add(row);
            }

            inner.Controls.Add(_edgeFlow);
            top += model.ActiveEdgeIndices.Count * 40 + 8;
        }

        if (model.ActiveVentIndices.Count > 0)
        {
            var lblVents = new Label
            {
                Location = new Point(0, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Text = L.Get("Setup.VentsHint")
            };
            inner.Controls.Add(lblVents);
            top += 28;

            _ventFlow.FlowDirection = FlowDirection.TopDown;
            _ventFlow.AutoSize = true;
            _ventFlow.WrapContents = false;
            _ventFlow.Location = new Point(0, top);
            _ventFlow.Width = 420;

            foreach (int ventIndex in model.ActiveVentIndices)
            {
                var row = new Panel { Width = 400, Height = 36 };
                row.Controls.Add(new Label
                {
                    Location = new Point(0, 8),
                    AutoSize = true,
                    Text = L.F("Setup.VentLabel", ventIndex)
                });

                var numVent = new NumericUpDown
                {
                    Location = new Point(200, 4),
                    Width = 110,
                    DecimalPlaces = 2,
                    Maximum = 99999,
                    Minimum = 0,
                    Value = (decimal)(model.VentSaByIndex.TryGetValue(ventIndex, out double ventSa) ? ventSa : 0)
                };
                _ventSaInputs[ventIndex] = numVent;
                row.Controls.Add(numVent);
                _ventFlow.Controls.Add(row);
            }

            inner.Controls.Add(_ventFlow);
        }

        scroll.Controls.Add(inner);

        var bottom = DialogUiHelper.CreateBottomButtonBar();

        _btnCancel.DialogResult = DialogResult.Cancel;
        DialogUiHelper.ConfigureButton(_btnCancel, 90);
        DialogUiHelper.ConfigureButton(_btnOk, 130);
        _btnOk.DialogResult = DialogResult.None;
        _btnOk.Click += OnOkClick;

        bottom.Controls.Add(_btnCancel);
        bottom.Controls.Add(_btnOk);

        Controls.Add(scroll);
        Controls.Add(bottom);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.EditImportedCsvRow");
        _lblHint.Text = L.Get("Import.EditHint");
        _lblKalinlik.Text = L.Get("Setup.GlassThickness");
        _lblAdet.Text = L.Get("Setup.DesiredQty");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.SaveRecipeChanges");
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        _model.KalinlikMm = (double)_numKalinlik.Value;
        _model.Adet = (int)_numAdet.Value;

        foreach (var (edgeIndex, num) in _edgeSaInputs)
            _model.SaByEdge[edgeIndex] = (double)num.Value;

        foreach (var (edgeIndex, num) in _edgeOffsetInputs)
            _model.OffsetByEdge[edgeIndex] = (double)num.Value;

        foreach (var (ventIndex, num) in _ventSaInputs)
            _model.VentSaByIndex[ventIndex] = (double)num.Value;

        DialogResult = DialogResult.OK;
    }
}
