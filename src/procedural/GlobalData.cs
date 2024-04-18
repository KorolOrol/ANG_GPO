﻿using System;

public class GlobalData
{
    public static int[,] tabl = new int[,]
    {
        {0, 0, 3, 2, 3, 2, 2, 2, 3, 1, 2, 1, 1, 3, 2, 2, 0, 3, 3, 1, 2, 3, 1, 1, 1, 3, 3, 2, 1, 2, 1, 2, 0, 3, 1, 1, 3, 3, 1, 1, 2},
        {0, 0, 1, 3, 2, 2, 3, 1, 1, 3, 1, 2, 1, 1, 2, 1, 3, 0, 2, 1, 2, 0, 2, 1, 2, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1, 2, 2, 2, 2, 1},
        {3, 1, 0, 0, 2, 2, 2, 2, 2, 1, 2, 1, 2, 1, 2, 2, 1, 1, 2, 2, 1, 2, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 2, 3, 1, 3, 1},
        {2, 3, 0, 0, 1, 2, 1, 1, 1, 1, 2, 2, 1, 2, 1, 3, 1, 2, 3, 1, 1, 2, 1, 2, 0, 3, 2, 1, 1, 2, 3, 2, 1, 1, 1, 1, 3, 2, 1, 1, 2},
        {3, 2, 2, 1, 0, 0, 2, 1, 2, 2, 2, 1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 0, 2, 1, 1, 0, 0, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 0, 1, 1, 1},
        {2, 2, 2, 2, 0, 0, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 2, 1, 2, 0, 3, 1, 1, 2, 2, 3, 2, 1, 2, 2, 2, 1, 2, 2, 0, 2, 3, 1, 2, 1},
        {2, 3, 2, 1, 2, 1, 0, 0, 2, 1, 1, 1, 1, 1, 2, 1, 3, 1, 2, 1, 2, 1, 2, 1, 1, 3, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1, 2, 1, 1, 1, 1},
        {2, 1, 2, 1, 1, 1, 0, 0, 1, 1, 2, 2, 1, 2, 3, 2, 2, 3, 1, 2, 2, 3, 1, 1, 1, 3, 2, 1, 1, 1, 1, 1, 1, 1, 3, 3, 3, 3, 1, 1, 1},
        {3, 1, 2, 1, 2, 1, 2, 1, 0, 0, 1, 1, 1, 2, 2, 1, 2, 2, 2, 1, 3, 1, 2, 1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 2, 2, 2, 2, 3, 1, 1, 1},
        {1, 3, 1, 1, 2, 1, 1, 1, 0, 0, 1, 1, 1, 2, 1, 2, 1, 2, 1, 1, 2, 2, 1, 3, 1, 2, 1, 2, 1, 1, 2, 3, 2, 1, 2, 3, 1, 1, 1, 1, 1},
        {2, 1, 2, 2, 2, 2, 1, 2, 1, 1, 0, 0, 2, 1, 1, 3, 1, 1, 1, 3, 1, 3, 1, 1, 2, 2, 1, 1, 2, 1, 1, 1, 2, 2, 1, 1, 3, 3, 2, 2, 1},
        {1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 3, 2, 1, 1, 2, 0, 2, 1, 2, 1, 1, 2, 0, 2, 2, 1, 1, 1, 1, 1},
        {1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2, 2, 0, 0, 2, 1, 2, 0, 3, 2, 1, 3, 1, 0, 2, 1, 2, 0, 3, 1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 3, 0},
        {3, 1, 1, 2, 1, 1, 1, 2, 2, 2, 1, 1, 0, 0, 1, 2, 1, 3, 2, 1, 1, 3, 2, 2, 0, 3, 2, 2, 1, 2, 1, 3, 1, 2, 3, 3, 3, 1, 1, 1, 2},
        {2, 2, 2, 1, 2, 2, 2, 3, 2, 1, 1, 1, 2, 1, 0, 0, 2, 1, 2, 1, 2, 2, 2, 1, 2, 2, 3, 1, 2, 2, 1, 2, 1, 2, 2, 2, 2, 1, 2, 2, 1},
        {2, 1, 2, 3, 2, 1, 1, 2, 1, 2, 3, 1, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 1, 2, 0, 2, 2, 2, 1, 1, 2, 2, 2, 2, 1, 1, 1, 2, 1, 2, 2},
        {0, 3, 1, 1, 2, 1, 3, 2, 2, 1, 1, 2, 2, 1, 2, 1, 0, 0, 1, 1, 2, 0, 1, 1, 1, 1, 2, 1, 2, 1, 1, 2, 3, 1, 0, 3, 2, 1, 1, 1, 1},
        {3, 0, 1, 2, 1, 2, 1, 3, 2, 2, 1, 1, 0, 3, 1, 2, 0, 0, 1, 1, 1, 2, 1, 2, 1, 3, 3, 3, 1, 2, 2, 3, 0, 3, 3, 1, 2, 3, 2, 0, 3},
        {3, 2, 2, 3, 1, 1, 2, 1, 2, 1, 1, 1, 3, 2, 2, 1, 1, 1, 0, 0, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 0, 3, 2, 1, 3, 1, 2, 0, 2, 2, 2},
        {1, 1, 2, 1, 1, 2, 1, 2, 1, 1, 3, 1, 2, 1, 1, 1, 1, 1, 0, 0, 2, 3, 0, 2, 2, 1, 1, 1, 1, 1, 3, 3, 1, 3, 2, 2, 1, 3, 1, 1, 1},
        {2, 2, 1, 1, 2, 0, 2, 2, 3, 2, 1, 2, 1, 1, 2, 2, 2, 2, 2, 1, 0, 0, 2, 1, 1, 1, 0, 1, 1, 2, 1, 3, 1, 1, 1, 3, 1, 1, 1, 1, 2},
        {3, 0, 2, 2, 0, 3, 1, 3, 1, 2, 3, 3, 3, 3, 2, 2, 0, 2, 1, 3, 0, 0, 0, 2, 1, 3, 3, 1, 1, 3, 3, 3, 1, 2, 3, 3, 1, 3, 1, 2, 1},
        {1, 2, 1, 1, 2, 1, 2, 1, 2, 1, 1, 2, 1, 2, 2, 1, 1, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 1, 1, 1, 0, 3, 2, 1, 3, 3, 2, 1, 1, 2, 2},
        {1, 1, 1, 2, 1, 1, 1, 1, 1, 3, 1, 1, 0, 2, 1, 2, 1, 2, 1, 2, 0, 2, 0, 0, 1, 3, 1, 1, 1, 1, 2, 2, 2, 1, 1, 2, 1, 2, 1, 1, 1},
        {1, 2, 2, 0, 1, 2, 1, 1, 1, 1, 2, 1, 2, 0, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 3, 1, 2, 2, 1, 1, 0, 3, 3, 3, 0, 2, 1},
        {3, 1, 1, 3, 0, 2, 3, 3, 2, 2, 2, 2, 1, 3, 2, 2, 1, 3, 1, 1, 1, 3, 2, 3, 0, 0, 3, 1, 2, 1, 3, 3, 1, 2, 1, 3, 3, 3, 1, 1, 1},
        {3, 1, 1, 2, 0, 3, 2, 2, 2, 1, 1, 0, 2, 2, 3, 2, 2, 3, 1, 1, 0, 3, 1, 1, 0, 3, 0, 0, 1, 1, 1, 2, 1, 3, 1, 1, 2, 3, 1, 2, 1},
        {2, 1, 1, 1, 2, 2, 1, 1, 1, 2, 1, 2, 0, 2, 1, 2, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 2, 1, 1, 1, 1, 2, 0, 1, 2, 1, 2, 1},
        {1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2, 1, 3, 1, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 3, 2, 1, 0, 0, 0, 1, 2, 2, 1, 0, 0, 2, 1, 1, 2, 3},
        {2, 1, 2, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 2, 1, 1, 2, 1, 1, 2, 3, 1, 1, 1, 1, 1, 2, 0, 0, 2, 2, 1, 2, 3, 3, 2, 3, 1, 3, 2},
        {1, 1, 1, 3, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 0, 3, 1, 3, 0, 2, 2, 3, 1, 1, 1, 2, 0, 0, 1, 2, 2, 2, 0, 1, 1, 0, 1},
        {2, 2, 1, 2, 1, 2, 2, 1, 2, 3, 1, 1, 1, 3, 2, 2, 2, 3, 3, 3, 3, 3, 3, 2, 2, 3, 2, 1, 2, 2, 0, 0, 1, 3, 2, 3, 2, 3, 1, 3, 2},
        {0, 2, 1, 1, 2, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 3, 0, 2, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1, 0, 0, 0, 0, 2, 0, 1, 1, 2},
        {3, 1, 1, 1, 1, 2, 1, 1, 2, 1, 2, 0, 1, 2, 2, 2, 1, 3, 1, 3, 1, 2, 1, 1, 1, 2, 3, 1, 1, 2, 2, 3, 0, 0, 3, 3, 3, 3, 2, 2, 3},
        {1, 1, 1, 1, 1, 2, 1, 3, 2, 2, 1, 2, 2, 3, 2, 1, 0, 3, 3, 2, 1, 3, 3, 1, 0, 1, 1, 2, 0, 3, 2, 2, 0, 3, 0, 3, 2, 3, 1, 1, 2},
        {1, 1, 1, 1, 1, 0, 1, 3, 2, 3, 1, 2, 1, 3, 2, 1, 3, 1, 1, 2, 3, 3, 3, 2, 3, 3, 1, 0, 0, 3, 2, 3, 0, 3, 3, 0, 3, 2, 1, 1, 1},
        {3, 2, 2, 3, 1, 2, 2, 3, 2, 1, 3, 1, 2, 3, 2, 1, 2, 2, 2, 1, 1, 1, 2, 1, 3, 3, 2, 1, 2, 2, 0, 2, 2, 3, 2, 3, 0, 3, 0, 3, 3},
        {3, 2, 3, 2, 0, 3, 1, 3, 3, 1, 3, 1, 2, 1, 1, 2, 1, 3, 0, 3, 1, 3, 1, 2, 3, 3, 3, 2, 1, 3, 1, 3, 0, 3, 3, 2, 3, 0, 3, 0, 3},
        {1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 2, 2, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 0, 3, 0, 1, 1},
        {1, 2, 3, 1, 1, 2, 1, 1, 1, 1, 2, 1, 3, 1, 2, 2, 1, 0, 2, 1, 1, 2, 2, 1, 2, 1, 2, 2, 2, 3, 0, 3, 1, 2, 1, 1, 3, 0, 1, 0, 0},
        {2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 0, 2, 1, 2, 1, 3, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 3, 2, 1, 2, 2, 3, 2, 1, 3, 3, 1, 0, 0},
    };

    public static List<Trait> traits_list = new List<Trait>()
    {
        new Trait("Амбициозный", new List<string> // Ambitious
        {
            "Имеющий или проявляющий притязания на достижение значительных целей, больших успехов.",
            "Имеющий или демонстрирующий амбиции, т.е. притязания на успех в соответствии с большими запросами и целями.",
            "Ставит перед собой высокие цели, стремится к успеху и постоянно работает над развитием своих навыков."

        }),
        new Trait("Приземлённый", new List<string> // Content
        {
            "Трезво оценивающий реальную действительность, избегающий иллюзий.",
            "Не витающий в облаках, избегающий мечтательности и идеализаций",
            "Реалистично оценивает свои возможности, умеет правильно расставлять приоритеты и не боится повседневных рутинных задач."
        }),
             
        

        new Trait("Храбрый", new List<string> // Brave
        {
            "Не знающий страха, решительный.",
            "Не знает страха в минуту опасности, способен преодолеть его.",
            "Легко рискующий своей жизнью ради достижения цели или спасения другого человека."
        }),
        new Trait("Трусливый", new List<string> // Craven
        {
            "Робкий, боязливый.",
            "Склонен к боязни и панике, избегает опасных ситуаций.",
            "Боящийся даже незначительных опасностей."
        }),
                 
        

        new Trait("Спокойный", new List<string> // Calm
        {
            "Не подверженный частым сменам настроения, уравновешенный, невозмутимый.",
            "Не теряющий самообладания даже в сложных ситуациях, умеющий контролировать свои эмоции.",
            "Хладнокровный, уравновешенный, не поддающийся эмоциональным всплескам."
        }),
        new Trait("Гневный", new List<string> // Wrathful
        {
            "Испытывающий или выражающий гнев, сильно сердящийся.",
            "Часто приходящий в состояние гнева, легко поддающийся негативным эмоциям.",
            "Охваченный, пребывающий в состоянии гнева, раздражения, злости."
        }),
                                     
        new Trait("Целомудренный", new List<string> // Chastle
        {
            "Отличающийся непорочностью, чистотой, строгостью в нравственном отношении.",
            "Соблюдающий строгие нормы морали и нравственности, избегающий всего, что может быть истолковано как нарушение этих норм.",
            "Характеризующийся непорочностью, чистотой, неповреждённый греховным опытом."
        }),
        new Trait("Похотливый", new List<string> // Lustful
        {
            "Испытывающий сильное половое влечение, сладострастный.",
            "Испытывающий чрезмерное половое влечение, одержимый сексуальными желаниями.",
            "Вожделеющий, чувственный, движимый похотью."
        }),
                 
        

        new Trait("Усердный", new List<string> // Diligent
        {
            "Всегда старается выполнить задачу наилучшим образом и доводит дело до конца.",
            "Всегда стремится к наилучшему выполнению задач, трудолюбив и не останавливается на достигнутом.",
            "Всегда доводит дело до конца, не боится трудностей и при необходимости готов потратить больше усилий для достижения результата."
        }),
        new Trait("Ленивый", new List<string> // Lazy
        {
            "Предпочитает отдых активной деятельности, склонен откладывать дела на потом.",
            "Не всегда склонен к активной деятельности, скорее предпочитает отдых работе, может быть менее успешен в делах.",
            "Избегает лишней нагрузки, предпочитает не тратить много сил на выполнение задач и может быть менее продуктивным по сравнению с другими людьми."
        }),
                   
        

        new Trait("Общительный", new List<string> // Generous
        {
            "Легко идёт на контакт, любит проводить время в компании, часто становится душой компании.",
            "Открыт к взаимодействию, любит общество и легко находит общий язык с другими людьми.",
            "Легко находит контакт с людьми, открытый и дружелюбный в общении, с удовольствием делится эмоциями и впечатлениями."
        }),
        new Trait("Стеснительный", new List<string> // Shy
        {
            "Испытывает неловкость и смущение в обществе других людей, предпочитает оставаться в тени.",
            "Может испытывать сложности в общении, иногда ему может быть нелегко заводить новые знакомства и открываться людям.",
            "Иногда испытывает трудности в начале общения, предпочитает более осторожный подход к взаимодействию с окружающими."
        }),
           
        

        new Trait("Щедрый", new List<string> // Gregarious
        {
            "Готовый делиться своими ресурсами, временем, энергией и вниманием.",
            "Готов делиться своими благами с окружающими, не ожидая ничего взамен",
            "Способный с радостью отдавать своё время, деньги, внимание и ресурсы окружающим."
        }),
        new Trait("Жадный", new List<string> // Greedy
        {
            "Стремящийся получить как можно больше, не желающий делиться имеющимися ресурсами.",
            "Стремящийся к накоплению материальных благ и неохотно расстающийся с ними.",
            "Испытывающий чрезмерную скупость, нежелание делиться и отдавать что-либо другим людям."
        }),
                 
        

        new Trait("Честный", new List<string> // Honest
        {
            "Говорит правду и не скрывает своих намерений.",
            "Не скрывает правду, говорит откровенно и искренне.",
            "С высокими моральными принципами, уважающий истину и не склонный к обману или предательству."
        }),
        new Trait("Лживый", new List<string> // Deceitful
        {
            "Склонен искажать информацию и вводить других в заблуждение.",
            "Склонен искажать факты и выдавать желаемое за действительное.",
            "Склонный говорить неправду и манипулировать информацией ради собственных интересов или выгоды."
        }),
                               
        

        new Trait("Скромный", new List<string> // Humble
        {
            "Предпочитающий не привлекать к себе лишнего внимания и не хвастаться своими достижениями.",
            "Предпочитает оставаться в тени и не привлекать к себе внимания.",
            "Избегает привлечения внимания к себе, предпочитая оставаться в тени."
        }),
        new Trait("Высокомерный", new List<string> // Arrogant
        {
            "Часто демонстрирующий своё превосходство над другими и относящийся к ним с пренебрежением.",
            "Ставит себя выше других и относится к ним с пренебрежением.",
            "Демонстрирует превосходство и пренебрежение к окружающим, полагая себя лучше других."
        }),
        
        

        new Trait("Взвешенный", new List<string> // Just
        {
            "Принимает решения, основываясь на тщательном анализе и оценке всех возможных последствий.",
            "Принимает решения, основываясь на анализе ситуации и возможных последствий.",
            "Принимает решения после тщательного анализа ситуации и возможных последствий."
        }),
        new Trait("Взбалмошный", new List<string> // Arbitrary
        {
            "Склонен к неожиданным и импульсивным поступкам, не всегда задумываясь о последствиях.",
            "Действует импульсивно и непредсказуемо, руководствуясь своими эмоциями.",
            "Действует непредсказуемо и импульсивно, не задумываясь о последствиях своих действий."
        }),
               
        

        new Trait("Терпеливый", new List<string> // Patient
        {
            "Способен спокойно ждать и переносить трудности.",
            "Способен сохранять спокойствие и выдержку даже в самых сложных ситуациях.",
            "Способен спокойно и без раздражения ожидать результата или события, которое обязательно произойдёт."
        }),
        new Trait("Нетерпеливый", new List<string> // Impatient
        {
            "С трудом переносит ожидание и стремится получить желаемое как можно скорее.",
            "Склонен быстро терять терпение и раздражаться из-за ожидания или медленного выполнения задач.",
            "С трудом переносит задержку в действиях, проявляя излишнюю поспешность и эмоциональность."
        }),
                           
        

        new Trait("Сдержанный", new List<string> // Temperate
        {
            "Контролирует свои эмоции и не позволяет им проявляться слишком сильно.",
            "Умеющий контролировать свои эмоции и действия, проявляя самообладание и уравновешенность.",
            "Контролирует свои эмоции, мысли и действия, избегая чрезмерной экспрессии или проявления своих чувств."
        }),
        new Trait("Прожорливый", new List<string> // Gluttonous
        {
            "Ест много и часто, не контролируя количество съеденного.",
            "Испытывает сильное желание есть, не контролируя количество потребляемой пищи.",
            "Испытывает постоянную потребность в еде, часто переедает, что может привести к проблемам со здоровьем."
        }),
                 
        

        new Trait("Доверчивый", new List<string> // Trusting
        {
            "Легко верит людям и полагается на них.",
            "Легко полагается на слова и действия других, не ожидая обмана.",
            "Легко верит людям и часто бывает открытым для отношений и сотрудничества."
        }),
        new Trait("Параноик", new List<string> // Paranoid
        {
            "Склонен видеть угрозу и опасность в самых безобидных ситуациях.",
            "Склонен видеть угрозу и заговор в обычных событиях, чрезмерно опасаясь за свою безопасность.",
            "Склонен видеть угрозы и заговоры там, где их нет, и может иметь проблемы с общением из-за недоверия к окружающим"
        }),
                             
        

        new Trait("Ревностный", new List<string> // Zealous
        {
            "Проявляет чрезмерную заботу и внимание к объекту своего интереса.",
            "Страстно относится к своим убеждениям и интересам, проявляя сильное желание защитить их от посягательств.",
            "Обладает сильным чувством преданности, которое иногда может перерастать в чрезмерную ответственность или навязчивость."
        }),
        new Trait("Циничный", new List<string> // Cynical
        {
            "Склонен к критическому и негативному восприятию мира и людей.",
            "Скептически относится к мотивам и действиям других людей, полагая, что все руководствуются эгоистичными интересами.",
            "Лишенный наивности, что позволяет быть более рациональным."
        }),
                
        

        new Trait("Сочувствующий", new List<string> // Compassionate
        {
            "Всегда готов поддержать и помочь другим.",
            "Способен поддержать и понять чужие переживания.",
            "Легко сопереживает горю и радости других."
        }),
        new Trait("Жестокий", new List<string> // Callous
        {
            "Не проявляет сострадания и может причинить боль другим.",
            "Не склонен к состраданию и может причинить боль окружающим.",
            "Не проявляет сострадания к боли и страданиям окружающих."
        }),
            
        

        new Trait("Переменчивый", new List<string> // Fickle
        {
            "Быстро меняет свои решения и интересы.",
            "Быстро меняет свои взгляды и решения.",
            "Быстро меняет свое мнение и настроение."
        }),
        new Trait("Упёртый", new List<string> // Stubborn
        {
            "Настойчиво идёт к своей цели, несмотря на препятствия.",
            "Настойчив в достижении своих целей и не отступает перед трудностями.",
            "Настойчиво отстаивает свои идеи и принципы, несмотря на возражени."
        }),
            
        

        new Trait("Заурядный", new List<string> // Ordinary
        {
            "Предпочитает не выделяться из толпы и не стремится к оригинальности.",
            "Лишённый оригинальности, не отличающийся от других.",
            "Не выделяется из толпы, предпочитая сливаться с окружением."
        }),
        new Trait("Эксцентричный", new List<string> // Eccentric
        {
            "Удивляет окружающих своим необычным поведением и стилем жизни.",
            "Привлекает внимание нестандартным поведением и образом жизни.",
            "Вызывающе оригинальный, необычный."
        }),
            
        

        new Trait("Садист", new List<string> // Sadistic
        {
            "Получает удовольствие от причинения боли и страданий другим людям.",
            "Получающий удовольствие от причинения боли и страданий другим.",
            "Испытывает наслаждение, причиняя боль другим людям или живым существам."
        }),
        new Trait("Мазохист", new List<string> // Masochism
        {
            "Испытывает удовольствие от причинения боли и страданий самому себе.",
            "Испытывающий удовлетворение от самобичевания и получения боли от других.",
            "Получает удовлетворение от собственных страданий или самобичевания."
        }),
            
        

        new Trait("Религиозный", new List<string> // Religious
        {
            "Придерживающийся определенной религии и исповедующий ее принципы в своей жизни.",
            "Верующий в Бога или богов и следующий религиозным практикам и убеждениям.",
            "Верит в божественное и следует религиозным доктринам и обрядам."
        }),
        new Trait("Фанатик", new List<string> // Fanatic
        {
            "Слепо следующий убеждениям и не воспринимающий другие точки зрения.",
            "Страстно преданный своей вере или идее.",
            "Слепо следующий определённым идеям или кумирам и готовый ради них идти на крайние меры."
        }),
        new Trait("Атеист", new List<string> // Aetheist
        {
            "Не верящий в существование Бога или богов",
            "Отрицающий существование Бога или богов.",
            "Неверующий."
        }),
                          
        

        new Trait("Справедливый", new List<string> // Fair
        {
            "Руководствующийся принципами равенства и честности в своих поступках.",
            "Всегда старается действовать в соответствии с моральными принципами и правами других людей.",
            "Руководствующийся моральными принципами и правилами."
        }),
        new Trait("Несправедливый", new List<string> // Unfair
        {
            "Склонный к нарушению прав других людей и пренебрежению моральными нормами.",
            "Не соблюдает моральные принципы и права других людей и действует в своих интересах.",
            "Поступающий вопреки нормам морали и нарушающий правила."
        }),
    };

    public static int charactersCreated = 0;
}