   M S B A M L   `   `   `  ���� P  LPalindromeCheckClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:PalindromeCheckClientPalindromeCheckClient  S.clr-namespace:PalindromeCheckClient.ViewModels PalindromeCheckClient.ViewModels  &     PalindromeCheckClient.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5local#clr-namespace:PalindromeCheckClient     E
ViewModels.clr-namespace:PalindromeCheckClient.ViewModels     -     �Name$  MainWin�� � Title$P JПроверка текстов на наличие палиндромов��$	��450��$	��800��$	��200��$	��300�� � DataContext :  4PalindromeCheckClient.ViewModels.MainWindowViewModel  .��� .+��� $��
10,63,0,10q�.���  � ColumnDefinitions �� $��* ��� $	��160 �.+�&   System.Windows.Controls.DataGrid  $��0��$��Stretch=�$��Stretch=�   AutoGenerateColumns .     GridLinesVisibility$ 
Horizontal=�$   HorizontalScrollBarVisibility$
 Auto=�   
IsReadOnly .    HeadersVisibility$ Column=����� *DGFilesItems+	 �� UpdateSourceTrigger$	 PropertyChanged=�
 �� Mode$
 OneWay=� �� NotifyOnTargetUpdated .  �� NotifyOnSourceUpdated .    Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$ *    Header$! Текстовый файл��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Text+$	 PropertyChanged=�$
 OneWay=� .  .   $��1��$��Stretch=�$��Stretch=� .  $ 
Horizontal=�$
 Auto=� .    CanUserSortColumns .     CanUserReorderColumns .     CanUserResizeRows .  $ Column=����� *DGSimTPalItems+$	 PropertyChanged=�$
 OneWay=� .  .    $ * $2 ,Является ли палиндромом�� �� *SimilarityTPalString+$	 PropertyChanged=�$
 OneWay=� .  . �� -    ��Name$ FolderPathTbx��$��23��$��
10,10,90,0q�$	��Top=� � ToolTip$G AПуть к папке с проверяемыми файлами�� �� 
IsReadOnly . $��Center=����� *
FolderPath+$
 OneWay=��� -    ��Name$ FolderPathBtn��$��
Обзор��$��	0,10,10,0q�$��Right=�$��75��$��23��$	��Top=�
 � Tag$+ %Указать путь к папке��{��� *FolderPathBtnIsEnabled+���� *OpenCommand+�� -   $ URITbx��$��23��$��
10,35,90,0q�$	��Top=�$P JАдрес сервера. По умолчанию: http://127.0.0.1:8080/��$��Center=����� *URI+$
 TwoWay=��� -   $ CheckPalindromeBtn��$��Проверить��$��	0,35,10,0q�$	��Top=�$��23��$��Right=�$��75��$R LПроверить содержимое файлов на палиндром��{��� *CheckPalindromeBtnIsEnabled+���� *CheckCommand+