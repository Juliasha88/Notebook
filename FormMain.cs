using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace Notebook
{
    public partial class FormMain : Form
    {

        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Открытие формы для добавления нового контакта
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormEdit FormForEdit;
            FormForEdit = new FormEdit();
            FormForEdit.Text = "Добавление новой записи";

            DataClassesDataContext dbContextContext = new DataClassesDataContext();

            //перед добавлением нового контакта сохраняем количество контактов
            int countItems = dbContextContext.Person.Count();
            FormForEdit.ShowDialog();
            RefreshData();
            //если контактов стало больше, значит точно добавили новый. Находим его по максимальному индекса и выделяем
            if (dbContextContext.Person.Count() > countItems)
                SelectedPerson(dbContextContext.Person.Max(p => p.ID));
        }

        /// <summary>
        /// Выбор контакта в списке для его выделения и отображения списка телефонов
        /// </summary>
        /// <param name="idPerson">ID контакта, который нужно выделить</param>
        public void SelectedPerson(long? idPerson)
        {

            //buttonDelete.Focus();
            if (idPerson != null)
            {
                int settedFocus = listView.Items.IndexOfKey(idPerson.ToString());
                if (settedFocus != -1)
                {
                    listView.Items[settedFocus].Selected = true;
                    listView_Click(listView, null);
                    listView.Items[settedFocus].Focused = true;
                    listView.Focus();
                }
            }

        }

        /// <summary>
        /// Обновление списка контактов на экране
        /// </summary>
        public void RefreshData()
        {
            DataClassesDataContext dbContextContext = new DataClassesDataContext();
            ShowListPerson(dbContextContext.Person);
        }


        /// <summary>
        /// Удаление контакта
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if ((listView.SelectedIndices.Count != 0))
            {
                DataClassesDataContext dbContext = new DataClassesDataContext();

                //получение ID контакта, который необходимо удалить
                int PersonID = int.Parse(listView.Items[listView.SelectedItems[0].Index].Name);
                Person person;

                //получаем контакт, который надо удалить
                person = dbContext.Person.Where(p => p.ID == PersonID).FirstOrDefault();
                DialogResult dialogResult = MessageBox.Show($"Удалить {person.Name} {person.Surname} ?", "Удаление", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    dbContext.Person.DeleteOnSubmit(person);
                    dbContext.SubmitChanges();
                    RefreshData();
                }
            }

        }


        /// Отображает список номеров телефонов для выбранного контакта
        private void listView_Click(object sender, EventArgs e)
        {
            int itemIndex = listView.SelectedItems[0].Index;
            int PersonID = int.Parse(listView.Items[itemIndex].Name);

            DataClassesDataContext dbContext = new DataClassesDataContext();
            listBox.DataSource = dbContext.Phone.Where(p => p.PersonID == PersonID);
            listBox.DisplayMember = "Number";
            listBox.ValueMember = "ID";
        }


        /// <summary>
        /// Открывает форму для редактирования контакта
        /// </summary>
        /// <param name="PersonID">ID контакта, который будет редактироваться</param>
        private void OpenPersonEdit(int PersonID)
        {
            DataClassesDataContext dbContext = new DataClassesDataContext();
            Person person = dbContext.Person.First(p => p.ID == PersonID);

            FormEdit FormForEdit;
            FormForEdit = new FormEdit();
            FormForEdit.Text = "Редактирование";

            //передаем во вторую форму контакт, который будем редактировать
            FormForEdit.SelectedPerson = person;

            FormForEdit.ShowDialog();

            //обновляем данные после редактирования
            RefreshData();

            //выделяем отредактированный контакт
            SelectedPerson(PersonID);

        }

        /// <summary>
        /// Открывает окно для редактировани контакта
        /// </summary>
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if ((listView.SelectedIndices.Count != 0))
            {
                int itemIndex = listView.SelectedItems[0].Index;
                int PersonID = int.Parse(listView.Items[itemIndex].Name);

                OpenPersonEdit(PersonID);
            }

        }

        /// <summary>
        /// Возвращает коллекцию контактов, у которых имя содержит name
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        /// <param name="name">Искомое имя</param>
        /// <returns></returns>
        private IEnumerable<Person> SearchByName(DataClassesDataContext dbContext, string name)
        {
            return dbContext.Person.Where(p => p.Name.Contains(name));
        }

        /// <summary>
        /// Возвращает коллекцию контактов, у которых фамилия содержит surname
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        /// <param name="name">Искомая фамилия</param>
        /// <returns></returns>
        private IEnumerable<Person> SearchBySurname(DataClassesDataContext dbContext, string surname)
        {
            return dbContext.Person.Where(p => p.Surname.Contains(surname));
        }

        /// <summary>
        /// Возвращает коллекцию контактов, у которых номер телефона содержит phoneNum
        /// </summary>
        /// <param name="dbContext">Контекст базы данных</param>
        /// <param name="name">Искомый номер телефона</param>
        /// <returns></returns>
        private IEnumerable<Person> SearchByPhone(DataClassesDataContext dbContext, string phoneNum)
        {
            return from person in dbContext.Person
                   join phone in dbContext.Phone on person.ID equals phone.PersonID
                   where phone.Number.Contains(phoneNum)
                   select person;
        }

        /// <summary>
        /// Отображает на экране коллекцию всех контактов
        /// </summary>
        /// <param name="person">Коллекция контактов</param>
        private void ShowListPerson(IEnumerable<Person> person)
        {
            listView.Items.Clear();

            if (person != null)
            {
                foreach (var _person in person)
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Text = _person.Name;
                    lvi.SubItems.Add(_person.Surname);
                    lvi.SubItems.Add(_person.BirthYear.ToString());

                    //задаем ID контакта, чтобы потом по этому ID определять выбранный в listView контакт
                    lvi.Name = _person.ID.ToString();

                    // добавляем элемент в ListView
                    listView.Items.Add(lvi);
                }
            }
            //чтобы очистить список телефонов
            listBox.DataSource = null;
        }

        /// <summary>
        /// Возвращает коллекцию контактов, у которых в имени, фамилии и телефоне есть вхождение searchString
        /// </summary>
        /// <param name="searchString">Искомый текст</param>
        /// <returns></returns>
        private IEnumerable<Person> SearchPerson(string searchString)
        {
            //Поиск идет отдельно в именах, фамилиях, телефонах. 
            //Затем эти коллекции объединяются, и выводятся только уникальные контакты
            DataClassesDataContext dbContext = new DataClassesDataContext();
            IEnumerable<Person> byName, bySurname, byPhone;
            byName = SearchByName(dbContext, searchString);
            bySurname = SearchBySurname(dbContext, searchString);
            byPhone = SearchByPhone(dbContext, searchString);

            return byName.Union(bySurname).Union(byPhone);
        }

        //При вводе каждого символа осуществляется поиск среди контактов
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            ShowListPerson(SearchPerson(textBox.Text));
        }

        //private bool CheckConnectionOnDatabase()
        //{
        //    try
        //    {
        //        DataClassesDataContext dbContext = new DataClassesDataContext();
        //        //dbContext.Database.OpenConnection();
        //        //dbContext.Database.CloseConnection();
        //        if (dbContext.DatabaseExists())
        //            return true;
        //        else return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        private void FormMain_Load(object sender, EventArgs e)
        {
            //ловим исключение, вдруг нет соединения с сервером БД или БД отсутствует 
            try
            {
                RefreshData();
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                MessageBox.Show("Не удалось подключиться к базе данных");
                Application.Exit();
            }


            listView.Columns[0].Width = 100;
            listView.Columns[1].Width = 200;
            listView.Columns[2].Width = 90;   
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            int itemIndex = listView.SelectedItems[0].Index;
            int PersonID = int.Parse(listView.Items[itemIndex].Name);

            //Открываем окно для редактирования контакта
            OpenPersonEdit(PersonID);
        }
    }
}
