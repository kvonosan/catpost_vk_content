   M S B A M L   `   `   `  ���� M  Icatpost_vk_content, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:Catpost_VK_ContentCatpost_VK_Content  #    Catpost_VK_Content.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:Catpost_VK_Content     -     � Title$  Catpost VK Content 3.2��$	��500��$
��1000�� � 	Resources    
WhiteBrush �� Color  NormalBrush �� 
StartPoint �� EndPoint � Color � Offset  HorizontalNormalBrush  
LightBrush  HorizontalLightBrush  	DarkBrush  PressedBrush  DisabledForegroundBrush  DisabledBackgroundBrush 	 WindowBackgroundBrush 
 SelectedBackgroundBrush  NormalBorderBrush  HorizontalNormalBorderBrush  DefaultedBorderBrush  PressedBorderBrush  DisabledBorderBrush  SolidBorderBrush  LightBorderBrush  
GlyphBrush  LightColorBrush �� 
TargetType �� Property	 �� Value
 �� 
TargetType �� CornerRadius ��Name �� Triggers P� Property P� Value �� 
TargetName ��Name��%�	  &	        &	      &	 g     &	 �     &	     &	 i    &	 �    &	 I    &	 Y    &		 i    &	
 y    &	 �    &	 �    &	 5    &	 �    &	 �    &	 �    &	     &	     &	 !    '�� 1    7   7  7   7  7  7  '�� �    7  7  7  7  7  7  7  7  7  7  �� $ White���� $	 0,0&�$	 0,1&���� � $
 #FFF��$	 0.0Q�� $
 #CCC��$	 1.0Q��� $	 0,0&�$	 1,0&���� � $
 #FFF��$	 0.0Q�� $
 #CCC��$	 1.0Q��� $	 0,0&�$	 0,1&���� � $
 #FFF��$	 0.0Q�� $
 #EEE��$	 1.0Q��� $	 0,0&�$	 1,0&���� � $
 #FFF��$	 0.0Q�� $
 #EEE��$	 1.0Q��� $	 0,0&�$	 0,1&���� � $
 #FFF��$	 0.0Q�� $
 #AAA��$	 1.0Q��� $	 0,0&�$	 0,1&���� � $
 #BBB��$	 0.0Q�� $
 #EEE��$	 0.1Q�� $
 #EEE��$	 0.9Q�� $
 #FFF��$	 1.0Q��� $
 #888���� $
 #EEE���� $
 #FFF���� $
 #DDD���� $	 0,0&�$	 0,1&���� � $
 #CCC��$	 0.0Q�� $
 #444��$	 1.0Q��� $	 0,0&�$	 1,0&���� � $
 #CCC��$	 0.0Q�� $
 #444��$	 1.0Q��� $	 0,0&�$	 0,1&���� � $
 #777��$	 0.0Q�� $
 #000��$	 1.0Q��� $	 0,0&�$	 0,1&���� � $
 #444��$	 0.0Q�� $
 #888��$	 1.0Q��� $
 #AAA���� $
 #888���� $
 #AAA���� $
 #444���� $
 #DDD���� " ��.���  � ��	 �� "
 ��.R�� .+��� $��Border��$��0,0,-4,0q�8��  8�� $��1,1,1,1q�$ 2,12,0,0��._��� $ ContentSite��$��Center=�$��Center=�$��Header��$��	12,2,12,2q���.  P�  �@��
IsSelected . . ���  �@K�ZIndex$		 100���� $ Border��8	   � ���� $ Border�� � ��$	 1,1,1,0q�P�  � {� .  . ��� $ Border��8	   � ���� $ Border��8	   � ���� 8	   � ���� " ��.���  �@J�SnapsToDevicePixels	 . ��  �@�OverridesDefaultStyle	 . ��  � ��$	 23����  � ��$	 75����  � ��	 �� "
 ��.R��� $ Border��$ 2��$��1q�8��  8�� ._��� $��2q�$��Center=�$��Center=���.  P�  �@J�IsKeyboardFocused . . ��� $ Border��8	   � ��P�  �@��IsDefaulted . . ��� $ Border��8	   � ��P�  �@J�IsMouseOver . . ��� $ Border��8	   � ��P�  � �� . . ��� $ Border��8	   � ���� $ Border��8	   � ��P�  � {� .  . ��� $ Border��8	   � ���� $ Border��8	   � ���� 8	 	  � ��.��� .+��� -    ��Name$	 tab��.@��� ����  �� Orientation$ 
Horizontal=�
�������.+��� 
�������Посты вконтакте�� -    ��Name$ stack_posts��$ 
Horizontal=�
�������.���  � RowDefinitions �� $��40 ��� $��* ��� $��40 �.+��� -    ��Name$ sort_button��$��0��$,��&Сортировать посты по��$	��140��$��20��$
��Left=�$	��Top=�$��	20,10,0,0q��� -    ��Name$ 
sort_label��$��0��$
��Left=�$	��Top=�$��
170,10,0,0q�$/��)По дате - новое вначале���� -   $ category_button��$��0��$��Категории��$��60��$��20��$
��Left=�$	��Top=�$��
460,10,0,0q��� -   $ update_button��$��0��$��Обновить��$��60��$��20��$
��Left=�$	��Top=�$��
540,10,0,0q��� -    ��Name$ 	favorites��$��0��$	��120��$��20��$
��Left=�$	��Top=�$��
620,10,0,0q��� -	    ��Name$ scroll��$��1��.���� -
   $ stack��$��1��$��150, 0q��� -   $
 prev��$��2��$��Bottom=�$��	20,0,0,10q�$
��Left=�{�.  .��!Предыдущие посты�� -   $
 next��$��2��$��Bottom=�$��
160,0,0,10q�$
��Left=�.��Следующие посты�� -   $ posts_count_label��$��2��$��
300,0,0,10q�$��Bottom=�$
��Left=�$
��Bold"��� ���� $ 
Horizontal=�
�������.+��� 
�������Новости�� -   $ 
stack_news��$ 
Horizontal=�
�������.���� -   $ scroll_news��.���� -   $
 news���� ���� $ 
Horizontal=�
�������.+��� 
�������Гороскоп�� -   $ stack_goroskope��$ 
Horizontal=�
�������.���� -   $ scroll_goroskope��.���� -   $ 	goroskope���� ���� $ 
Horizontal=�
�������.+��� 
�������Погода�� -   $ stack_pogoda��$ 
Horizontal=�
�������.���� -   $ scroll_pogoda��.���� .+��� .+��� $3��-Введите название города:��$��Center=��� -    ��Name$ pogoda_text�� �� MaxLines$ 1�� �� TextWrapping$ NoWrap=� �� AcceptsReturn .  $��
10,20,10,0q�$	��200���� -   $ pogoda_button��$��Загрузить��$��80��$��20��$��Center=�$	��Top=�$��0,20,0,0q��� -   $ pogoda���� 
����������� $ 
Horizontal=�
�������.+��� 
�������Вокруг света�� -   $ stack_vokrug��$ 
Horizontal=�
�������.���� -   $ scroll_vokrug��.���� -   $ vokrug_sveta��