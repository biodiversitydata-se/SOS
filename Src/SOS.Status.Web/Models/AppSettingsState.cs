using static SOS.Status.Web.Components.Pages.TaxaFilter;

namespace SOS.Status.Web.Models;

public class AppSettingsState
{
    public event Action OnChange;

    private List<Taxon> selectedTaxa = new();
    public IReadOnlyList<Taxon> SelectedTaxa => selectedTaxa;

    public void AddTaxon(Taxon taxon)
    {
        if (!selectedTaxa.Any(t => t.Id == taxon.Id))
        {
            selectedTaxa.Add(taxon);
            NotifyStateChanged();
        }
    }

    public void RemoveTaxon(Taxon taxon)
    {
        if (selectedTaxa.Remove(taxon))
        {
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
