# Лабораторная работа 6: Создание внутренней формы представления программы


# Цель работы: 
Изучить методы построения внутреннего представления программы (ВПП) на основе контекстно-свободной грамматики, реализовать синтаксический анализатор методом рекурсивного спуска и преобразовать арифметические выражения в тетрады и ПОЛИЗ.

Сведения об авторе 

Студент: Тарбаев Даба-Цырен 

Группа: АП-327


# Постановка задачи
1. Реализовать поиск лексических и синтаксических ошибок для заданной КС-грамматики методом рекурсивного спуска.
2. Представить внутреннюю форму программы в виде тетрад (op, arg1, arg2, result) для арифметических выражений (только для корректных строк).
3. Преобразовать выражение в ПОЛИЗ (польскую инверсную запись) и вычислить его значение (только арифметическое выражение из целых чисел).


# Вариант задания 

Грамматика арифметических выражений для языков программирования: 16. C#


P:
1. E → TA
2. A → ε | + TA | - TA
3. T → FB
4. B → ε | * FB | / FB | % FB
5. F → num | id | (E)
6. id → letter {letter | digit | _}
7. num → digit {digit}

Z = E

VT = { a-zA-Z, 0-9, _, +, -, *, /, %, (, ) }

VN = { E, A, T, B, F, id, num }


Классификация Хомскомо: контекстно-свободные грамматики:
A -> α, A ∊ VN, α ∊ V* 

примеры верных строк

    a + b * c - d / ((a * b)+a)
    15/(7-(1+1))*3-(2+(1+1))
    1 + 2 - 3 % ( 5 * 3 - 13) / 1

# Диаграмма лексера
<img width="2856" height="3621" alt="Тфяк-лаб6-сканер" src="https://github.com/user-attachments/assets/86ad9379-3e8e-4e43-8ad2-5bb37e5ce99f" />

# Схема рекурсивного спуска для парсера
<img width="2018" height="2477" alt="Тфяк-лаб6-парсер" src="https://github.com/user-attachments/assets/6a379534-bf30-4aef-8dd0-49517a7b09b2" />

# тестовые примеры для лексера
<img width="1043" height="732" alt="изображение" src="https://github.com/user-attachments/assets/d5d49a2c-f608-4f8c-960f-4428326a7fcb" />
Рисунок 1 - Пример корректной строки

<img width="1042" height="728" alt="изображение" src="https://github.com/user-attachments/assets/33dc1be1-6b3e-4989-9ad0-a874cc564216" />
Рисунок 2 - Пример строки c недопустимыми символами

# тестовые примеры для парсера
<img width="1048" height="725" alt="изображение" src="https://github.com/user-attachments/assets/a0cd3991-c871-45bb-aa5e-d30b885dbd10" />
Рисунок 3 - Пример корректной строки

<img width="1045" height="732" alt="изображение" src="https://github.com/user-attachments/assets/2654414e-7847-4479-8a9f-ee648bab7408" />
Рисунок 4 - Пример строки с пропушенными знаками и операнд

<img width="1041" height="730" alt="изображение" src="https://github.com/user-attachments/assets/05357d6a-beb6-472a-b3d4-b2afb29275a8" />
Рисунок 5 - Пример строки с лишними скобками и недопустимыми символами из лексера

<img width="1043" height="725" alt="изображение" src="https://github.com/user-attachments/assets/f5d22df6-d5c5-4ba0-a86e-91f631b447c8" />
Рисунок 6 - Пример строки с недостоющими скобками 

# Внутренняя форма представления программы (тетрады): 

<img width="1043" height="590" alt="изображение" src="https://github.com/user-attachments/assets/e6c26449-b377-4542-aa9e-5b6222915cef" />
Рисунок 7 - пример №1

<img width="1039" height="570" alt="изображение" src="https://github.com/user-attachments/assets/534fe678-4aad-4f85-8a99-df10e4485025" />
Рисунок 8 - пример №2

<img width="1042" height="603" alt="изображение" src="https://github.com/user-attachments/assets/24edfeac-c935-4de0-b599-fd2d22cf3eff" />
Рисунок 9 - пример №3

# ПОЛИЗ
<img width="1040" height="605" alt="изображение" src="https://github.com/user-attachments/assets/95328205-2903-468a-8659-0d34e8d74d40" />
Рисунок 10 - пример №1

<img width="1043" height="605" alt="изображение" src="https://github.com/user-attachments/assets/a604c6e0-51d8-4fbd-9262-6cf3c571a1ca" />
Рисунок 11 - пример №2

<img width="1037" height="608" alt="изображение" src="https://github.com/user-attachments/assets/8cbca333-7792-4f8b-8d7f-7b1a25e5ebff" />
Рисунок 12 - пример №3
