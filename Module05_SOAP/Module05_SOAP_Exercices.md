# Module 05 - SOAP

## Exercice 1 - Service de base

- Créez la solution Visual Studio "DSED_M05_Ex01" de type "Application web ASP.Net Core". Vous pouvez prendre le gabarit vide
- Ajoutez le projet "DSED_M05_Model" de type "Bibliothèque de classes"
- Créez le service "Operations" qui propose les opérations sur des "float" suivantes :
  - Addition
  - Soustraction
  - Multiplication
  - Division
  - Racine carré
- Faites un GET de l'adresse de votre service et observez le fichier WSDL généré
- Créez un client qui permet de tester votre service

## Exercice 2 - Municipalités

- Reprenez votre solution qui contient l'API de manipulation des municipalités
- Implantez un service qui correspond à toutes les méthodes du dépot
- Faites un GET de l'adresse de votre service et observez le fichier WSDL généré
- Créez un client qui permet de tester votre service. Attention, si vous allez chercher toutes les municipalités, vous allez avoir un problème de taille de messages. Afin de ne pas l'avoir, il faut modifier la propriété "MaxReceivedMessageSize" avec une valeur de 10Mo (1024 * 1024 * 10) de l'objet "BasicHttpBinding".

<details>
    <summary>Diagramme de package global</summary>

![Diagramme UML](../images/Module05_SOAP/diag/M05_Exercice2_packages/M05_Exercice2_SOAP_package.png)
</details>

<details>
    <summary>Diagramme de classe global</summary>

Dans ce diagramme, les packages sont les projets, les sous-packages des dossiers.

![Diagramme UML](../images/Module05_SOAP/diag/M05_Exercice2/M05_Exercice2_SOAP.png)
</details>
