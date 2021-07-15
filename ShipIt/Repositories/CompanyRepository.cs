﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface ICompanyRepository
    {
        int GetCount();
        CompanyDataModel GetCompany(string gcp);
        IEnumerable<CompanyDataModel> GetCompaniesByGcp(List<string> gcp);
        void AddCompanies(IEnumerable<Company> companies);
        Dictionary<string, Company> GetCompanyDictionary(Dictionary<int, Product> productDictionary);
    }

    public class CompanyRepository : RepositoryBase, ICompanyRepository
    {
        public int GetCount()
        {
            string CompanyCountSQL = "SELECT COUNT(*) FROM gcp";
            return (int)QueryForLong(CompanyCountSQL);
        }

        public CompanyDataModel GetCompany(string gcp)
        {
            string sql =
                "SELECT gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail " +
                "FROM gcp " +
                "WHERE gcp_cd = @gcp_cd";
            var parameter = new NpgsqlParameter("@gcp_cd", gcp);
            string noProductWithIdErrorMessage = string.Format("No companies found with gcp: {0}", gcp);
            return base.RunSingleGetQuery(sql, reader => new CompanyDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public IEnumerable<CompanyDataModel> GetCompaniesByGcp(List<string> gcp)
        {
            string sql = String.Format("SELECT gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail " +
                "FROM gcp " +
                "WHERE gcp_cd IN ('{0}')", String.Join("','", gcp));
            return base.RunGetQuery(sql, reader => new CompanyDataModel(reader), "No companies found with given gcps", null);
        }

        public void AddCompanies(IEnumerable<Company> companies)
        {
            string sql =
                "INSERT INTO gcp (gcp_cd, gln_nm, gln_addr_02, gln_addr_03, gln_addr_04, gln_addr_postalcode, gln_addr_city, contact_tel, contact_mail) " +
                "VALUES (@gcp_cd, @gln_nm, @gln_addr_02, @gln_addr_03, @gln_addr_04, @gln_addr_postalcode, @gln_addr_city, @contact_tel, @contact_mail)";

            var parametersList = new List<NpgsqlParameter[]>();
            foreach (var company in companies)
            {
                var companyDataModel = new CompanyDataModel(company);
                parametersList.Add(companyDataModel.GetNpgsqlParameters().ToArray());
            }

            base.RunTransaction(sql, parametersList);
        }

        public Dictionary<string, Company> GetCompanyDictionary(Dictionary<int, Product> productDictionary)
        {
            var companyGcp = new List<string>();
            foreach (var product in productDictionary)
            {
                companyGcp.Add(product.Value.Gcp);
            }
            var companies = this.GetCompaniesByGcp(companyGcp);
            var companyDictionary = new Dictionary<string, Company>();
            foreach (var company in companies)
            {
                companyDictionary.Add(company.Gcp, new Company(company));
            }
            return companyDictionary;
        }
    }

}