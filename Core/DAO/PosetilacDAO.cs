using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.DAO;


namespace Core.DAO
{
    public class PosetilacDAO
    {
        private readonly string fajl;

        public PosetilacDAO()
        {
            fajl = Core.Utils.DataPaths.File("posetioci.txt");
        }
        private KnjigaDAO knjigaDao = new KnjigaDAO();
        public List<Posetilac> Ucitaj()
        {
            List<Posetilac> posetioci = new List<Posetilac>();
            if (!File.Exists(fajl))
                return posetioci;
            string[] linije = File.ReadAllLines(fajl);
            for (int i = 0; i < linije.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(linije[i]))
                    continue;
                string[] delovi = linije[i].Split('|');
                if (delovi.Length < 13) continue;

                Posetilac pos = new Posetilac();
                pos.Ime = delovi[0];
                pos.Prezime = delovi[1];
                pos.DatRodj = DateTime.Parse(delovi[2]);
                pos.Ad = new Adresa();
                pos.Ad.Ulica = delovi[3];
                pos.Ad.Broj = int.Parse(delovi[4]);
                pos.Ad.Grad = delovi[5];
                pos.Ad.Drzava = delovi[6];
                pos.BrojTelefona = delovi[7];
                pos.Mail = delovi[8];
                pos.CK = delovi[9];
                pos.GodC = int.Parse(delovi[10]);
                pos.Status = (StatusPosetioca)Enum.Parse(typeof(StatusPosetioca), delovi[11]);
                pos.ProsecnaOcena = double.Parse(delovi[12]);
                if (delovi.Length > 13 && !string.IsNullOrWhiteSpace(delovi[13]))
                {
                    string listaZelja = delovi[13];          // npr "9781,9782,9783"
                    string[] isbnovi = listaZelja.Split(',');

                    List<Knjiga> sveKnjige = knjigaDao.Load();

                    for (int j = 0; j < isbnovi.Length; j++)
                    {
                        string isbn = isbnovi[j];

                        for (int k = 0; k < sveKnjige.Count; k++)
                        {
                            if (sveKnjige[k].ISBN == isbn)
                            {
                                pos.ListaZelja.Add(sveKnjige[k]);
                                break;
                            }
                        }
                    }
                }

                pos.Validacija();
                posetioci.Add(pos);

            }
            return posetioci;
        }

        public void Sacuvaj(List<Posetilac> posetioci)
        {
            List<string> linije = new List<string>();
            for (int i = 0; i < posetioci.Count; i++)
            {
                Posetilac pos = posetioci[i];
                string listaZelja = "";
                for (int j = 0; j < pos.ListaZelja.Count; j++)
                {
                    if (j > 0)
                        listaZelja = listaZelja + ",";

                    listaZelja = listaZelja + pos.ListaZelja[j].ISBN;
                }
                string tekst = pos.Ime + '|' + pos.Prezime + '|' + pos.DatRodj.ToString("yyyy-MM-dd") + '|' + pos.Ad.Ulica + '|' + pos.Ad.Broj + '|' + pos.Ad.Grad + '|' + pos.Ad.Drzava + '|' + pos.BrojTelefona + '|' + pos.Mail + '|' + pos.CK + '|' + pos.GodC + '|' + pos.Status + '|' + pos.ProsecnaOcena +'|'+ listaZelja;
                linije.Add(tekst);
            }
            File.WriteAllLines(fajl, linije);
        }
    }
}
