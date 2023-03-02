using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Esempio_Controllore_Semafori
{
    internal class Esempio_Controllore_Semafori
    {
        // DA NON MODIFICARE: inizio
        static Semafori.frmSemafori fSemafori;
        static gamon.IO.Digital.IoSimulation IO;
        // DA NON MODIFICARE: fine

        static short portaIn = 0;   // primi 8 bit di I/O (0 .. 7)
        static short portaOut1 = 1; // secondi 8 bit di I/O (8 .. 15)
        static short portaOut2 = 2; // terzi 8 bit di I/O (16 .. 23)

        private static void MostraSimulazione()
        {
            // DA NON MODIFICARE: inizio
            // dalla .Show() il form non esce mai in questo thread
            fSemafori = new Semafori.frmSemafori();
            IO = (gamon.IO.Digital.IoSimulation)fSemafori.frmDigitalIO.Hardware;
            fSemafori.Show();
            // DA NON MODIFICARE: fine
        }
        static void Main(string[] args)
        {
            // DA NON MODIFICARE: inizio
            // fa partire il sistema simulato
            Thread t1 = new Thread(MostraSimulazione);
            t1.Start();
            Thread.Sleep(5000); // attende per dare tempo all'altro thread di inizializzare tutto
            // DA NON MODIFICARE: fine

            // semafori rossi: 
            IO.Out(portaOut1, 0b1000_0000);
            IO.Out(portaOut2, 0b0100_0011);

            Thread.Sleep(8000); // temporaneo per test: attende che arrivi qualcuno in coda

            // metto verde i due semafori
            bool giallo = false;
            IO.Out(portaOut1, 0b0011_1000);
            IO.Out(portaOut2, 0b0000_0010);
            while (true)
            {
                byte controlla_coda = IO.In(portaIn);
                controlla_coda &= 0b0001_1000; // metto a 0 i bit che non mi interessano

                if(controlla_coda == 0b0001_1000 ) // controlo se ci sono macchine in fila
                {
                    // prima metto i semafori a giallo
                    if (!giallo) { 
                        IO.Out(portaOut1, 0b0000_0000);
                        IO.Out(portaOut2, 0b0001_1110);
                        Thread.Sleep(2000);// aspetto 2 secondi con il semaforo giallo
                         giallo = true;
                    }

                    IO.Out(portaOut1, 0b1100_0000); // metto verde il semaforo sud e il resto a rosso
                    IO.Out(portaOut2, 0b0100_0001);
                }

                if(controlla_coda == 0b0000_0000)   // controlla che non ci siano più macchine in coda a SUD
                {
                    // prima metto i semafori a giallo
                    if (giallo)
                    {
                        IO.Out(portaOut1, 0b1000_0000);
                        IO.Out(portaOut2, 0b0110_0000);
                        Thread.Sleep(2000);
                        giallo = false;
                    }

                    IO.Out(portaOut1, 0b0011_1000);
                    IO.Out(portaOut2, 0b0000_0011);
                }


                /*
                IO.Out(portaOut1, 0x28);   // verde nei semafori più importanti rosso negli altri 
                IO.Out(portaOut2, 2);      // verde nei semafori più importanti rosso negli altri 

                while ((IO.In(portaIn) & 5) != 0) ; // attesa che entrambe le code diventino vuote
                */

            }
        }
    }
}
