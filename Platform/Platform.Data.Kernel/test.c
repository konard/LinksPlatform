#include "Link.h"
#include "PersistentMemoryManager.h"
#include <stdio.h>

int main() {

    printf("Startup.\n");

    OpenLinks("db.links");

    printf("Database opened.\n");

    link_index isA = CreateLink(itself, itself, itself);
    link_index isNotA = CreateLink(itself, itself, isA);
    link_index link = CreateLink(itself, isA, itself);
    link_index thing = CreateLink(itself, isNotA, link);

    UpdateLink(isA, isA, isA, link); // После этого минимальное ядро системы можно считать сформированным

    DeleteLink(isA); // Одна эта операция удалит все 4 связи
    DeleteLink(thing);

    printf("Test ok.\n");

    CloseLinks();

    printf("Database closed.\n");

    return 0;
}
