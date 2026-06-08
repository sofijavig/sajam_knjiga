using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class PosetiociZaAutoraDialog : Window
    {
        private readonly List<Posetilac> _all;
        private List<Posetilac> _filtered;

        public PosetiociZaAutoraDialog(string title, List<Posetilac> posetioci)
        {
            InitializeComponent();

            TxtTitle.Text = string.IsNullOrWhiteSpace(title)
                ? ((TryFindResource("VisitorsForAuthor_HeaderDefault") as string) ?? "👥 Visitors")
                : title;

            _all = posetioci ?? new List<Posetilac>();
            _filtered = _all.ToList();

            DgPosetioci.ItemsSource = _filtered;
            UpdateCount();
        }

        private void TbSearch_KeyUp(object sender, KeyEventArgs e)
        {
            var q = (TbSearch.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                _filtered = _all.ToList();
            }
            else
            {
                _filtered = _all.Where(p =>
                    ContainsCI(p.CK, q) ||
                    ContainsCI(p.Ime, q) ||
                    ContainsCI(p.Prezime, q) ||
                    ContainsCI(p.Status.ToString(), q) ||
                    ContainsCI(p.Ad?.ToString(), q)
                ).ToList();
            }

            DgPosetioci.ItemsSource = null;
            DgPosetioci.ItemsSource = _filtered;
            UpdateCount();
        }

        private void UpdateCount()
        {
            var fmt = (TryFindResource("VisitorsForAuthor_TotalFormat") as string) ?? "Total: {0}";
            TxtCount.Text = string.Format(fmt, _filtered.Count);
        }

        private static bool ContainsCI(string? s, string q)
            => (s ?? "").Contains(q, StringComparison.OrdinalIgnoreCase);
    }
}