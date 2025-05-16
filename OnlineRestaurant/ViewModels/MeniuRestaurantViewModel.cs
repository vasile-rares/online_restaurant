using OnlineRestaurant.Commands;
using OnlineRestaurant.Models;
using OnlineRestaurant.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OnlineRestaurant.ViewModels
{
    public class MeniuRestaurantViewModel : BaseVM
    {
        private readonly PreparatService _preparatService;
        private readonly CategorieService _categorieService;
        private readonly IRestaurantDataService<Meniu> _meniuService;
        private readonly IRestaurantDataService<Alergen> _alergenService;

        private ObservableCollection<CategorieViewModel> _categorii;
        private string _cautareCuvantCheie = string.Empty;
        private bool _cautareContrariu;
        private bool _cautareDupaAlergen;
        private bool _esteInCautare;

        public ObservableCollection<CategorieViewModel> Categorii
        {
            get => _categorii;
            set => SetProperty(ref _categorii, value);
        }

        public string CautareCuvantCheie
        {
            get => _cautareCuvantCheie;
            set
            {
                if (SetProperty(ref _cautareCuvantCheie, value))
                {
                    ExecutaCautare();
                }
            }
        }

        public bool CautareContrariu
        {
            get => _cautareContrariu;
            set
            {
                if (SetProperty(ref _cautareContrariu, value))
                {
                    ExecutaCautare();
                }
            }
        }

        public bool CautareDupaAlergen
        {
            get => _cautareDupaAlergen;
            set
            {
                if (SetProperty(ref _cautareDupaAlergen, value))
                {
                    ExecutaCautare();
                }
            }
        }

        public bool EsteInCautare
        {
            get => _esteInCautare;
            set => SetProperty(ref _esteInCautare, value);
        }

        public ICommand ResetareCautareCommand { get; }

        public MeniuRestaurantViewModel(
            PreparatService preparatService,
            CategorieService categorieService,
            IRestaurantDataService<Meniu> meniuService,
            IRestaurantDataService<Alergen> alergenService)
        {
            _preparatService = preparatService;
            _categorieService = categorieService;
            _meniuService = meniuService;
            _alergenService = alergenService;

            Categorii = new ObservableCollection<CategorieViewModel>();
            ResetareCautareCommand = new RelayCommand(ResetareCautare);

            // Încărcăm datele inițiale
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var categorii = await _categorieService.GetAllAsync();
            var preparate = await _preparatService.GetAllAsync();
            var meniuri = await _meniuService.GetAllAsync();
            var alergeni = await _alergenService.GetAllAsync();

            Categorii.Clear();

            // Grupăm datele pe categorii pentru afișare
            foreach (var categorie in categorii)
            {
                var categorieVM = new CategorieViewModel
                {
                    IdCategorie = categorie.IdCategorie,
                    Nume = categorie.Nume
                };

                // Adăugăm preparatele din această categorie
                var preparateCategorie = preparate.Where(p => p.IdCategorie == categorie.IdCategorie).ToList();
                foreach (var preparat in preparateCategorie)
                {
                    var preparatVM = new ItemMeniuViewModel
                    {
                        Id = preparat.IdPreparat,
                        Tip = TipItemMeniu.Preparat,
                        Denumire = preparat.Denumire,
                        Pret = preparat.Pret,
                        CantitatePortie = preparat.CantitatePortie,
                        Disponibil = preparat.InStoc,
                        Imagini = new ObservableCollection<string>(
                            preparat.Fotografii.Select(f => f.Url).ToList()),
                        Alergeni = new ObservableCollection<string>(
                            preparat.PreparatAlergeni.Select(pa => pa.Alergen.Nume).ToList())
                    };
                    categorieVM.Items.Add(preparatVM);
                }

                // Adăugăm meniurile din această categorie
                var meniuriCategorie = meniuri.Where(m => m.IdCategorie == categorie.IdCategorie).ToList();
                foreach (var meniu in meniuriCategorie)
                {
                    // Verifică dacă toate preparatele din meniu sunt disponibile
                    bool meniuDisponibil = meniu.MeniuPreparate
                        .All(mp => mp.Preparat.InStoc);

                    var meniuVM = new ItemMeniuViewModel
                    {
                        Id = meniu.IdMeniu,
                        Tip = TipItemMeniu.Meniu,
                        Denumire = meniu.Denumire,
                        Pret = meniu.PretTotal,
                        Disponibil = meniuDisponibil,
                        // Pentru meniuri, concatenăm cantitățile preparatelor
                        DetaliiContinut = string.Join(", ", meniu.MeniuPreparate
                            .Select(mp => $"{mp.Preparat.Denumire} ({mp.Cantitate} g)")),
                        // Pentru meniuri, concatenăm alergenii unici din toate preparatele
                        Alergeni = new ObservableCollection<string>(
                            meniu.MeniuPreparate
                                .SelectMany(mp => mp.Preparat.PreparatAlergeni.Select(pa => pa.Alergen.Nume))
                                .Distinct()
                                .ToList())
                    };
                    
                    // Adăugăm o imagine implicită pentru meniu
                    meniuVM.Imagini.Add("/Images/Meniuri/default-meniu.jpg");
                    
                    categorieVM.Items.Add(meniuVM);
                }

                if (categorieVM.Items.Count > 0)
                {
                    Categorii.Add(categorieVM);
                }
            }
        }

        private void ExecutaCautare()
        {
            if (string.IsNullOrWhiteSpace(CautareCuvantCheie))
            {
                EsteInCautare = false;
                LoadDataAsync();
                return;
            }

            EsteInCautare = true;
            FiltreazaPreparateSiMeniuri();
        }

        private async void FiltreazaPreparateSiMeniuri()
        {
            var categorii = await _categorieService.GetAllAsync();
            var preparate = await _preparatService.GetAllAsync();
            var meniuri = await _meniuService.GetAllAsync();

            string cuvantCheie = CautareCuvantCheie.Trim().ToLower();

            // Filtru pentru preparate
            var preparateFiltrate = preparate.Where(p =>
            {
                bool contine;
                
                if (CautareDupaAlergen)
                {
                    // Căutare după alergen
                    contine = p.PreparatAlergeni.Any(pa => 
                        pa.Alergen.Nume.ToLower().Contains(cuvantCheie));
                }
                else
                {
                    // Căutare după denumire
                    contine = p.Denumire.ToLower().Contains(cuvantCheie);
                }

                // Inversăm rezultatul dacă se dorește negația
                return CautareContrariu ? !contine : contine;
            }).ToList();

            // Filtru pentru meniuri
            var meniuriFiltrate = meniuri.Where(m =>
            {
                bool contine;
                
                if (CautareDupaAlergen)
                {
                    // Căutare după alergen în preparatele meniului
                    contine = m.MeniuPreparate.Any(mp => 
                        mp.Preparat.PreparatAlergeni.Any(pa => 
                            pa.Alergen.Nume.ToLower().Contains(cuvantCheie)));
                }
                else
                {
                    // Căutare după denumire
                    contine = m.Denumire.ToLower().Contains(cuvantCheie);
                }

                // Inversăm rezultatul dacă se dorește negația
                return CautareContrariu ? !contine : contine;
            }).ToList();

            // Construim lista de categorii filtrate
            Categorii.Clear();

            // Găsim toate categoriile care au preparate sau meniuri filtrate
            var categoriiFiltrate = categorii
                .Where(c => 
                    preparateFiltrate.Any(p => p.IdCategorie == c.IdCategorie) || 
                    meniuriFiltrate.Any(m => m.IdCategorie == c.IdCategorie))
                .ToList();

            foreach (var categorie in categoriiFiltrate)
            {
                var categorieVM = new CategorieViewModel
                {
                    IdCategorie = categorie.IdCategorie,
                    Nume = categorie.Nume
                };

                // Adăugăm preparatele filtrate din această categorie
                foreach (var preparat in preparateFiltrate.Where(p => p.IdCategorie == categorie.IdCategorie))
                {
                    var preparatVM = new ItemMeniuViewModel
                    {
                        Id = preparat.IdPreparat,
                        Tip = TipItemMeniu.Preparat,
                        Denumire = preparat.Denumire,
                        Pret = preparat.Pret,
                        CantitatePortie = preparat.CantitatePortie,
                        Disponibil = preparat.InStoc,
                        Imagini = new ObservableCollection<string>(
                            preparat.Fotografii.Select(f => f.Url).ToList()),
                        Alergeni = new ObservableCollection<string>(
                            preparat.PreparatAlergeni.Select(pa => pa.Alergen.Nume).ToList())
                    };
                    categorieVM.Items.Add(preparatVM);
                }

                // Adăugăm meniurile filtrate din această categorie
                foreach (var meniu in meniuriFiltrate.Where(m => m.IdCategorie == categorie.IdCategorie))
                {
                    bool meniuDisponibil = meniu.MeniuPreparate
                        .All(mp => mp.Preparat.InStoc);

                    var meniuVM = new ItemMeniuViewModel
                    {
                        Id = meniu.IdMeniu,
                        Tip = TipItemMeniu.Meniu,
                        Denumire = meniu.Denumire,
                        Pret = meniu.PretTotal,
                        Disponibil = meniuDisponibil,
                        DetaliiContinut = string.Join(", ", meniu.MeniuPreparate
                            .Select(mp => $"{mp.Preparat.Denumire} ({mp.Cantitate} g)")),
                        Alergeni = new ObservableCollection<string>(
                            meniu.MeniuPreparate
                                .SelectMany(mp => mp.Preparat.PreparatAlergeni.Select(pa => pa.Alergen.Nume))
                                .Distinct()
                                .ToList())
                    };
                    
                    // Adăugăm o imagine implicită pentru meniu
                    meniuVM.Imagini.Add("/Images/Meniuri/default-meniu.jpg");
                    
                    categorieVM.Items.Add(meniuVM);
                }

                if (categorieVM.Items.Count > 0)
                {
                    Categorii.Add(categorieVM);
                }
            }
        }

        private void ResetareCautare()
        {
            CautareCuvantCheie = string.Empty;
            CautareContrariu = false;
            CautareDupaAlergen = false;
            EsteInCautare = false;
            LoadDataAsync();
        }
    }
} 