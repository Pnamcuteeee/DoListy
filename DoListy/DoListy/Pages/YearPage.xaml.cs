namespace DoListy.Pages;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;
using DoListy.ViewModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Audio;

public partial class YearPage : ContentPage
{
    Dictionary<DateTime, List<IView>> goalsList;
    List<Goal> realGoalsList;
    private readonly IAudioManager audioManager;
    IAudioPlayer naviPlayer {  get; set; }
    IAudioPlayer clickPlayer {  get; set; }
    
    IAudioPlayer doneGoalPlayer {  get; set; }
    public DateTime CurrentDate { get; set; }
    public YearPage(IAudioManager audioManager)
    {
        InitializeComponent();
        CurrentDate = DateTime.Now;
        yearLabel.Text = CurrentDate.Year.ToString();
        SetIniDisplayDate(CurrentDate);
        goalsList = new Dictionary<DateTime, List<IView>>();
        loadGoals();
        this.audioManager = audioManager;
        createPlayers();
    }

    async void createPlayers()
    {
        doneGoalPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("check.mp3"));
        clickPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("click.mp3"));
    }
    void loadGoals()
    {
        realGoalsList = App.appointmentRepo.GetGoals();
        int countGoal = realGoalsList.Count;
        string goalName="";
        DateTime goalInYear = CurrentDate;
        DateTime goalInYear1 = CurrentDate;
        int flag1 = 0;
        for (int i=0; i<countGoal; i++)
        {
            
            goalName = realGoalsList[i].Title;
            if (goalName.Length > 15)
            {
                goalName = goalName.Substring(0, 13) + "...";
            }
            var newButton = new Button
            {
                Text = goalName,
                StyleId = i.ToString(),
                WidthRequest = 175,
                BorderColor = Color.FromHex("#8CABFF"),
                BackgroundColor = Color.FromRgba(0, 0, 0, 0),
                HeightRequest = 40,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(10, 0, 50, 0)

            }; var tempColor = newButton.BorderColor;
          


            var newCB = new CheckBox
            {
                IsChecked = realGoalsList[i].isDone,
                StyleId = i.ToString()
               

        }; 
            //DisplayAlert("Hello", newCB.StyleId, "OK");
            newCB.CheckedChanged += (sender, e) =>
            {


                if (int.TryParse(newCB.StyleId, out int iTemp))
                {
                    if (newCB.IsChecked)
                    {
                        doneGoalPlayer.Play();
                        newButton.BorderColor = Color.FromArgb("#ff081b25");
                        tempColor = Color.FromArgb("#ff081b25");
                        newButton.TextColor = Colors.Gray;
                        realGoalsList[iTemp].isDone = true;     
                    }
                    else
                    {
                        newButton.BorderColor = Color.FromArgb("#ff8cabff");
                        tempColor = Color.FromArgb("#ff8cabff");
                        newButton.TextColor = Colors.White;
                        realGoalsList[iTemp].isDone = false;
                    }
                    App.appointmentRepo.UpdateGoal(realGoalsList[iTemp]);
                }
            };
            newButton.Pressed += (sender, e) =>
            {
                newButton.BorderColor = Colors.White;
            };

            var newGrid = new Grid();
            newGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            newGrid.Children.Add(newButton);
            newGrid.SetColumn(newButton, 0);
            newGrid.SetRow(newButton, 0);
            newGrid.Children.Add(newCB);
            newGrid.SetColumn(newCB, 0);
            newGrid.SetRow(newCB, 0);
            newCB.HorizontalOptions = LayoutOptions.End;
            goalInYear = new DateTime(realGoalsList[i].Year, 1, 1);
            if (goalsList.ContainsKey(goalInYear))
            {
                goalsList[goalInYear].Add(newGrid);
            }
            else
            {
                var newGoalList = new List<IView>();
                newGoalList.Add(newGrid);
                goalsList.Add(goalInYear, newGoalList);
            }
            flag1 = goalsList[goalInYear].Count - 1;
            if (realGoalsList[i].Year == CurrentDate.Year)
            {
                goalsListGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                goalsListGrid.Children.Add(newGrid);
                goalsListGrid.SetRow(newGrid, goalsListGrid.RowDefinitions.Count - 1);
                goalsListGrid.SetColumn(newGrid, 0);
            }
            newButton.Clicked += (sender, e) =>
            {
                clickPlayer.Play();
                if (int.TryParse(newButton.StyleId, out int value)) {
                    newButton.BorderColor = tempColor;
                    SetGoals viewGoal = new SetGoals();

                    viewGoal.goalTitleEntry.Text = realGoalsList[value].Title;
                    viewGoal.goalTitleEntry.IsEnabled = false;
                    viewGoal.yearNumericEntry.Value = realGoalsList[value].Year;
                    viewGoal.yearNumericEntry.IsEnabled = false;
                    viewGoal.goalNoteEntry.Text = realGoalsList[value].Notes;
                    viewGoal.goalNoteEntry.IsEnabled = false;
                    viewGoal.setGoalsCancelButton.IsVisible = false;
                    viewGoal.setGoalsCreateButton.IsVisible = false;
                    viewGoal.editGoalButton.IsVisible = true;
                    this.ShowPopup(viewGoal);
                    viewGoal.setGoalsSaveButton.Clicked += (sender, e) =>
                    {
                        ((Button)((Grid)goalsList[goalInYear][flag1]).Children[0]).Text = viewGoal.goalTitleEntry.Text;
                        ((Button)(((Grid)(goalsListGrid.Children[flag1])).Children[0])).Text = viewGoal.goalTitleEntry.Text;
                        goalName = viewGoal.goalTitleEntry.Text;
                        realGoalsList[value].Title = goalName;
                        realGoalsList[value].Notes = viewGoal.goalNoteEntry.Text;
                        if ((int)viewGoal.yearNumericEntry.Value != realGoalsList[i].Year)
                        {
                            goalInYear1 = new DateTime((int)viewGoal.yearNumericEntry.Value, 1, 1);
                            IView temp = goalsList[goalInYear][flag1];
                            goalsList[goalInYear].RemoveAt(flag1);
                            if (goalsList.ContainsKey(goalInYear1))
                            {
                                goalsList[goalInYear1].Add(temp);
                            }
                            else
                            {
                                var newGoalList = new List<IView>();
                                newGoalList.Add(temp);
                                goalsList.Add(goalInYear1, newGoalList);
                            }
                            goalsListGrid.RemoveAt(flag1);
                            realGoalsList[value].Year = goalInYear1.Year;
                        }
                        viewGoal.Close();

                    };
                    App.appointmentRepo.UpdateGoal(realGoalsList[value]);

                }

            };
        }
    }
    private void SetIniDisplayDate(DateTime currentDate)
    {
        DateTime temp = new DateTime(currentDate.Year, 1, 1);
        janMonthViewCalendar.DisplayDate = temp;
        temp=temp.AddMonths(1);
        febMonthViewCalendar.DisplayDate = temp;
        temp=temp.AddMonths(1);
        marMonthViewCalendar.DisplayDate = temp;
        temp=temp.AddMonths(1);
        aprMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        mayMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        junMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        julMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        augMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        sepMonthViewCalendar.DisplayDate = temp;
        temp= temp.AddMonths(1);
        octMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        novMonthViewCalendar.DisplayDate = temp;
        temp = temp.AddMonths(1);
        decMonthViewCalendar.DisplayDate = temp;

    }
    private void OnLeftArrowButtonClicked(object sender, EventArgs e)
    {
        clickPlayer.Play();
        leftArrowButton.Opacity = 1.0;
        yearLabel.Text = Convert.ToString(Convert.ToInt32(yearLabel.Text) - 1);
        ChangeYearOfDisplayDate(false);
        CurrentDate = janMonthViewCalendar.DisplayDate;
        goalsListGrid.Children.Clear();
        goalsListGrid.RowDefinitions.Clear();
        if (goalsList.ContainsKey(CurrentDate))
        {
            var numOfGoals = goalsList[CurrentDate].Count;
            for (int i = 0; i < numOfGoals; i++)
            {
                goalsListGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                goalsListGrid.Children.Add(goalsList[CurrentDate][i]);
                goalsListGrid.SetColumn(goalsList[CurrentDate][i], 0);
                goalsListGrid.SetRow(goalsList[CurrentDate][i], goalsListGrid.RowDefinitions.Count - 1);
            }
        }

    }
    private void OnRightArrowButtonClicked(object sender, EventArgs e)
    {
        clickPlayer.Play();
        rightArrowButton.Opacity = 1.0;
        yearLabel.Text = Convert.ToString(Convert.ToInt32(yearLabel.Text) + 1);
        ChangeYearOfDisplayDate(true);
        CurrentDate = janMonthViewCalendar.DisplayDate;
        goalsListGrid.Children.Clear();
        goalsListGrid.RowDefinitions.Clear();
        if (goalsList.ContainsKey(CurrentDate))
        {
            var numOfGoals = goalsList[CurrentDate].Count;
            for (int i = 0; i < numOfGoals; i++)
            {
                goalsListGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                goalsListGrid.Children.Add(goalsList[CurrentDate][i]);
                goalsListGrid.SetColumn(goalsList[CurrentDate][i], 0);
                goalsListGrid.SetRow(goalsList[CurrentDate][i], goalsListGrid.RowDefinitions.Count - 1);
            }
        }

    }
    private void ChangeYearOfDisplayDate(bool plus)
    {
        int num;
        if (plus) num = 1;
        else num = -1;
        janMonthViewCalendar.DisplayDate = janMonthViewCalendar.DisplayDate.AddYears(num);
        febMonthViewCalendar.DisplayDate = febMonthViewCalendar.DisplayDate.AddYears(num);
        marMonthViewCalendar.DisplayDate = marMonthViewCalendar.DisplayDate.AddYears(num);
        aprMonthViewCalendar.DisplayDate = aprMonthViewCalendar.DisplayDate.AddYears(num);
        mayMonthViewCalendar.DisplayDate = mayMonthViewCalendar.DisplayDate.AddYears(num);
        junMonthViewCalendar.DisplayDate = junMonthViewCalendar.DisplayDate.AddYears(num);
        julMonthViewCalendar.DisplayDate = julMonthViewCalendar.DisplayDate.AddYears(num);
        augMonthViewCalendar.DisplayDate = augMonthViewCalendar.DisplayDate.AddYears(num);
        sepMonthViewCalendar.DisplayDate = sepMonthViewCalendar.DisplayDate.AddYears(num);
        octMonthViewCalendar.DisplayDate = octMonthViewCalendar.DisplayDate.AddYears(num);
        novMonthViewCalendar.DisplayDate = novMonthViewCalendar.DisplayDate.AddYears(num);
        decMonthViewCalendar.DisplayDate = decMonthViewCalendar.DisplayDate.AddYears(num);
    }

    private async void OnJanButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = janMonthViewCalendar.DisplayDate;
    }
    private async void OnFebButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = febMonthViewCalendar.DisplayDate;
    }
    private async void OnMarButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = marMonthViewCalendar.DisplayDate;
    }
    private async void OnAprButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = aprMonthViewCalendar.DisplayDate;
    }
    private async void OnMayButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = mayMonthViewCalendar.DisplayDate;
    }
    private async void OnJunButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = junMonthViewCalendar.DisplayDate;
    }
    private async void OnJulButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = julMonthViewCalendar.DisplayDate;
    }
    private async void OnAugButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = augMonthViewCalendar.DisplayDate;
    }
    private async void OnSepButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = sepMonthViewCalendar.DisplayDate;
    }
    private async void OnOctButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = octMonthViewCalendar.DisplayDate;
    }
    private async void OnNovButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = novMonthViewCalendar.DisplayDate;
    }
    private async void OnDecButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Month");
        var monthPage = (MonthPage)Shell.Current.CurrentPage;
        monthPage.Scheduler.DisplayDate = decMonthViewCalendar.DisplayDate;
    }
    private void OnGoalsPlusButtonClicked(object sender, EventArgs e)
    {
        clickPlayer.Play();
        goalsPlusButton.Opacity = 1.0;
        SetGoals setGoalsPage = new SetGoals();
        setGoalsPage.IniYearNumericEntry(CurrentDate.Year);
        setGoalsPage.editGoalButton.IsVisible = false;

        this.ShowPopup(setGoalsPage);
        /*var newGoal = new Label
        {
            Text = "A Goal in " + janMonthViewCalendar.DisplayDate.ToString()
        };
        var currentdate = janMonthViewCalendar.DisplayDate;
        if (goalsList.ContainsKey(currentdate))
        {
            goalsList[currentdate].Add(newGoal);
        }
        else
        {
            var newGoalList = new List<IView>();
            newGoalList.Add(newGoal);
            goalsList.Add(currentdate, newGoalList);
        }
        goalsListStack.Children.Add(newGoal);
        */
    }
    private void OnLeftArrowButtonPressed(object sender, EventArgs e)
    {
        leftArrowButton.Opacity = 0.5;
    }
    private void OnRightArrowButtonPressed(Object sender, EventArgs e)
    {
        rightArrowButton.Opacity = 0.5;
    }
    private void OnGoalsPlusButtonPressed(Object sender, EventArgs e)
    {
        goalsPlusButton.Opacity = 0.5;
    }
    public void getCreatedGoal(string goalName, int year, string note)
    {
        Goal tempGoal = new ViewModel.Goal()
        {
            Title = goalName,
            Year = year,
            Notes = note,
            isDone = false
        };
        App.appointmentRepo.AddGoal(tempGoal) ; 
        realGoalsList.Add(tempGoal);
        goalsListGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        if (goalName.Length > 15)
        {
            goalName = goalName.Substring(0, 13) + "...";
        }
        var newButton = new Button
        {
            Text = goalName,

            WidthRequest = 175,
            BorderColor = Color.FromHex("#8CABFF"),
            BackgroundColor = Color.FromRgba(0, 0, 0, 0),
            HeightRequest = 40,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(10,0,50,0)
           
        }; var tempColor = newButton.BorderColor;



        var newCB = new CheckBox {
            IsChecked = false,
    };
        newCB.CheckedChanged += (sender, e) =>
        {
          

            if (newCB.IsChecked)
            {
                doneGoalPlayer.Play();
                newButton.BorderColor = Color.FromArgb("#ff081b25");
                tempColor = Color.FromArgb("#ff081b25");
                newButton.TextColor = Colors.Gray;
                realGoalsList[realGoalsList.Count - 1].isDone = true;
            }
            else
            {
                newButton.BorderColor = Color.FromArgb("#ff8cabff");
                tempColor = Color.FromArgb("#ff8cabff");
                newButton.TextColor = Colors.White;
                realGoalsList[realGoalsList.Count - 1].isDone = false;

            }
        };
        newButton.Pressed += (sender, e) =>
        {   
            newButton.BorderColor = Colors.White;
        }; 
       
        var newGrid = new Grid();
        newGrid.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
        newGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        newGrid.Children.Add(newButton);
        newGrid.SetColumn(newButton, 0);
        newGrid.SetRow(newButton, 0);
        newGrid.Children.Add(newCB);
        newGrid.SetColumn(newCB, 0);
        newGrid.SetRow(newCB, 0);
        newCB.HorizontalOptions = LayoutOptions.End;
        DateTime goalInYear = new DateTime(year, 1, 1);
        if (goalsList.ContainsKey(goalInYear))
        {
            goalsList[goalInYear].Add(newGrid);
        }
        else
        {
            var newGoalList = new List<IView>();
            newGoalList.Add(newGrid);
            goalsList.Add(goalInYear, newGoalList);
        }
        int flag1 = goalsList[goalInYear].Count-1;
        if (goalInYear.Year == CurrentDate.Year)
        {
            goalsListGrid.Children.Add(newGrid);
            goalsListGrid.SetRow(newGrid, goalsListGrid.RowDefinitions.Count - 1);
            goalsListGrid.SetColumn(newGrid, 0);
        }
        newButton.Clicked += (sender, e) =>
        {
            clickPlayer.Play();
            newButton.BorderColor = tempColor;
            SetGoals viewGoal = new SetGoals();
            
            viewGoal.goalTitleEntry.Text = goalName;
            viewGoal.goalTitleEntry.IsEnabled = false;
            viewGoal.yearNumericEntry.Value = year;
            viewGoal.yearNumericEntry.IsEnabled = false;
            viewGoal.goalNoteEntry.Text = note;
            viewGoal.goalNoteEntry.IsEnabled = false;
            viewGoal.setGoalsCancelButton.IsVisible = false;
            viewGoal.setGoalsCreateButton.IsVisible = false;
            viewGoal.editGoalButton.IsVisible = true;
            this.ShowPopup(viewGoal);
            viewGoal.setGoalsSaveButton.Clicked += (sender, e) =>
            {
                ((Button)((Grid)goalsList[goalInYear][flag1]).Children[0]).Text = viewGoal.goalTitleEntry.Text;
                ((Button)(((Grid)(goalsListGrid.Children[flag1])).Children[0])).Text = viewGoal.goalTitleEntry.Text;
                goalName = viewGoal.goalTitleEntry.Text;
                realGoalsList[realGoalsList.Count - 1].Title = goalName;
                note = viewGoal.goalNoteEntry.Text;
                realGoalsList[realGoalsList.Count - 1].Notes = note;
                if ((int)viewGoal.yearNumericEntry.Value != year)
                {
                    DateTime goalInYear1 = new DateTime((int)viewGoal.yearNumericEntry.Value, 1, 1);
                    IView temp = goalsList[goalInYear][flag1];
                    goalsList[goalInYear].RemoveAt(flag1);
                    if (goalsList.ContainsKey(goalInYear1))
                    {
                        goalsList[goalInYear1].Add(temp);
                    }
                    else
                    {
                        var newGoalList = new List<IView>();
                        newGoalList.Add(temp);
                        goalsList.Add(goalInYear1, newGoalList);
                    }
                    goalsListGrid.RemoveAt(flag1);
                    realGoalsList[realGoalsList.Count - 1].Year = goalInYear1.Year;

                }
                viewGoal.Close();
            };
        };
        App.appointmentRepo.UpdateGoal(realGoalsList[realGoalsList.Count - 1]);
    }
}


