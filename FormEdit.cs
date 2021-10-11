using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Data;
using System.Linq;

namespace Notebook
{
    public partial class FormEdit : Form
    {
        

        private Person selectedPerson; //редактируемая запись контакта
        public Person SelectedPerson // открытое свойство для задания редактируемого контакта
        {
            get { return selectedPerson; }
            set { selectedPerson = value; }
        }

 
        private DataClassesDataContext dbContext;

        public FormEdit()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Возвращает истину, если проверка на корректность года рождения прошла успешно
        /// </summary>
        /// <param name="bYear">Проверяемый год рождения</param>
        /// <param name="birthYear">Возвращаемый год рождения</param>
        /// <returns></returns>
        private bool IsValidBirthYear(string bYear,  out int? birthYear)
        {
            bool result = false;
            birthYear = null;

            //если строка пустая, то значит год рождения не заполнен, возвращаем null
            if (bYear == string.Empty)
            {
                result = true;   
            }
            else
            {  
                //год должен быть в диапазоне от текущий год - 110 лет до текущего года
                if ( (int.TryParse(bYear, out int by)) &&  (by <= DateTime.Now.Year) && (by >= DateTime.Now.Year - countYears) )
                {
                    birthYear = by;
                    result = true;
                }
            }      
            return result;
        }

        /// <summary>
        /// Сохраннение в БД нового или измененного контакта
        /// </summary>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            //истина, если этот контакт уже есть в dbContext'e
            //нужно, чтобы повторно не добавлять уже существующую сущность
            bool isExistPerson = false;

            Person EditedPerson = new Person();

            //если есть выбранный контакт, значит сейчас происходит его редактирование, а не создание нового
            if (selectedPerson != null)
            {
                EditedPerson = dbContext.Person.First(p => p.ID == selectedPerson.ID);
                isExistPerson = true;
            }
            else
            {
                //если создание нового, то проверяем список измененных сущностей
                //среди них может быть контакт, в который добавили номер телефона,
                //значит этот контакт и надо сохранить в БД
                var listInsrt = dbContext.GetChangeSet().Inserts;

                foreach (var p in listInsrt)
                {
                    if (p.GetType() == EditedPerson.GetType())
                    {
                        EditedPerson = (Person)p;
                        isExistPerson = true;
                        break;
                    }
                }
            }


            EditedPerson.Name = textBoxName.Text.Trim();
            EditedPerson.Surname = textBoxSurname.Text.Trim();

            if (EditedPerson.Name != string.Empty)
            {
                //проверяем год рождения на корректность
                if (IsValidBirthYear(comboBoxBirthYear.Text, out int? birthYear) == true)
                {
                    EditedPerson.BirthYear = birthYear;

                    //если этой сущности в контексте еще нет, то добавляем
                    if (isExistPerson == false)
                        dbContext.Person.InsertOnSubmit(EditedPerson);

                    dbContext.SubmitChanges();

                    DialogResult = DialogResult.OK;
                }
                else
                    MessageBox.Show("Некорректная дата рождения!");
            }
            else
                MessageBox.Show("Имя не может быть пустым!");  
        }


        //количество прошлых лет, для отображения списка годов рождений
        private readonly int countYears = 110;

        private void FormEdit_Load(object sender, EventArgs e)
        {
            dbContext = new DataClassesDataContext();
            
            //список лет для выбора года рождения
            IEnumerable<int> Years = Enumerable.Range(DateTime.Now.Year - countYears, countYears+1).Select(x => x);

            comboBoxBirthYear.DataSource = Years.ToList();
            comboBoxBirthYear.Text = "";

            listBox.DisplayMember = "Number";
            listBox.ValueMember = "ID";

            //Если форму открыли для редактирования существующего контакта
            if (selectedPerson != null)
            {
                textBoxName.Text = selectedPerson.Name;
                textBoxSurname.Text = selectedPerson.Surname;
                comboBoxBirthYear.Text = selectedPerson.BirthYear.ToString();

                listBox.DataSource = dbContext.Phone.Where(p => p.PersonID == selectedPerson.ID);
            }
        }


        /// <summary>
        /// Открытие формы для ввода нового телефона и добавление его к контакту
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormPhoneNumber FrmPhoneNumber = new FormPhoneNumber();

            FrmPhoneNumber.ShowDialog();

            if (FrmPhoneNumber.DialogResult == DialogResult.OK)
            {
                string phoneNumber = FrmPhoneNumber.PhoneNumber;
                if (phoneNumber != "")
                {
                    Person person = new Person();

                    if (selectedPerson != null)
                    {
                        person = dbContext.Person.Where(p => p.ID == selectedPerson.ID).FirstOrDefault();
                    }
                    else
                    {
                        //получаем список новых сущностей, подготовленных на вставку в БД
                        var listInsrt = dbContext.GetChangeSet().Inserts;

                        //ищем среди них сущность контакта (Person)?
                        //чтобы к нему добавить новый телефон
                        foreach (var p in listInsrt)
                        {
                            if (p.GetType() == person.GetType())
                            {
                                person = (Person)p;
                                break;
                            }
                        }

                    }
                    
                    Phone EditedPhone = new Phone();
                    EditedPhone.Number = phoneNumber;
                    EditedPhone.Person = person;

                    person.Phone.Add(EditedPhone);


                    dbContext.Phone.InsertOnSubmit(EditedPhone);

                    listBox.DataSource = person.Phone;


                    //по непонятной мне причине, почему-то в listBox 
                    //не отображается несколько вновь добавленных номеров, хотя в коллекции person.Phone они есть, 
                    //и в БД они добавляются, но не отображаются
                }
            }  
        }

        /// <summary>
        /// Открытие окна для редактирования номера телефона
        /// </summary>
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {

                FormPhoneNumber FrmPhoneNumber = new FormPhoneNumber();

                Phone phone = listBox.Items[listBox.SelectedIndex] as Phone;
                FrmPhoneNumber.PhoneNumber = phone.Number;

                FrmPhoneNumber.ShowDialog();
                if (FrmPhoneNumber.DialogResult == DialogResult.OK)
                {
                    string phoneNumber = FrmPhoneNumber.PhoneNumber;
                    if (phoneNumber != "")
                    {
                        Phone editedPhone = dbContext.Phone.Where(p => p.ID == phone.ID).FirstOrDefault();
                        Person person = dbContext.Person.Where(p => p.ID == phone.PersonID).FirstOrDefault();
                        editedPhone.Number = phoneNumber;
                        listBox.DataSource = person.Phone;
                    }
                }          
            }
        }

        /// <summary>
        /// Удаление номера телефона
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                Phone phone = listBox.Items[listBox.SelectedIndex] as Phone;
                long personID = phone.PersonID;
                DialogResult dialogResult = MessageBox.Show($"Удалить {phone.Number}?", "Удаление", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Phone deletedPhone = dbContext.Phone.Where(p => p.ID == phone.ID).FirstOrDefault();
                    Person person = dbContext.Person.Where(p => p.ID == personID).FirstOrDefault();
                    person.Phone.Remove(deletedPhone);

                    listBox.DataSource = person.Phone;
                }
            }
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            buttonEdit_Click(null, null);
        }

    }
}
