   M S B A M L   `   `   `  ����P  LPalindromeCheckClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:PalindromeCheckClientPalindromeCheckClient  5   	   &     PalindromeCheckClient.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:PalindromeCheckClient     5   	   -   5          �Name$  MainWin��5   	    � Title$ 
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      � .+�5
   
   �� $��0,63,0,0q�6   .��5      � $��0,0,0,0q�6    � ColumnDefinitions 5      �� 5      $��* �6'   �� 5      $	��120 �6'   .+�5      �� -   5        $��PaliFilesLBox��5   *   $��0��6   6*   �� 5      -   5        $��
IsPaliLBox��5   *   $��1��6   6*   5      5      �� 5   
   -   5         ��Name$ FolderPathTbx��5      $
��Left=�6)   $��23��6D   $��	10,10,0,0q�6P    �� TextWrapping$
 Wrap=�6c   $	��Top=�6w   $	��692��6�   6�   �� 5   
   -   5        $ URITbx��5      $
��Left=�6n   $��23��6�   $��	10,35,0,0q�6�   $
 Wrap=�6�   $	��Top=�6�   $	��692��6�   ��6"   ��  �� Path$	 URI� �� UpdateSourceTrigger$ PropertyChanged=� �� Mode$ TwoWay=�6�   �� 5   
   -   5         ��Name$ FolderPathBtn��5      $��
Обзор��6(   $
��Left=�68   $��
707,10,0,0q�6S   $	��Top=�6g   $��75��6   $��23��6�   �� 5   
   -   5        $ CheckPalindromeBtn��5      $��Проверить��6-   $
��Left=�6A   $��
707,35,0,0q�6\   $	��Top=�6p   $��75��6�   $��23��6�   5      5      