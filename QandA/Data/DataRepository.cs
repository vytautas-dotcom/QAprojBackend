using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using QandA.Models;
using System.Linq;

namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _db;

        public DataRepository(IConfiguration db)
        {
            _db = db.GetConnectionString("DefaultConnection");
        }


        //Read
        public AnswerGetResponse GetAnswer(int answerId)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection
                    .QueryFirstOrDefault<AnswerGetResponse>(@"EXEC dbo.Answer_Get_ByAnswerId @AnswerId = @AnswerId", 
                    new { AnswerId = answerId});
            }
        }

        public QuestionGetSingleResponse GetQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                var question = connection.
                    QueryFirstOrDefault<QuestionGetSingleResponse>(@"EXEC dbo.Question_GetSingle @QuestionId = @QuestionId",
                    new { QuestionId = questionId });
                if (question != null)
                {
                    question.Answers = connection.Query<AnswerGetResponse>(@"EXEC dbo.Answer_Get_ByQuestionId @QuestionId = @QuestionId",
                    new { QuestionId = questionId });
                }
                return question;
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions()
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(@"EXEC dbo.Question_GetMany");
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsWithAnswers()
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();

                var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();

                return connection.Query<QuestionGetManyResponse, AnswerGetResponse, QuestionGetManyResponse>
                    ("EXEC dbo.Question_GetMany_WithAnswers", map: (q, a) =>
                     {
                         QuestionGetManyResponse question;

                         if (!questionDictionary.TryGetValue(q.QuestionId, out question))
                         {
                             question = q;
                             question.Answers = new List<AnswerGetResponse>();
                             questionDictionary.Add(question.QuestionId, question);
                         }
                         question.Answers.Add(a);
                         return question;
                     }, splitOn: "QuestionId")
                    .Distinct()
                    .ToList();
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestionsBySearch(string search)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(@"EXEC dbo.Question_GetMany_BySearch @Search = @Search", new { Search = search });
            }
        }

        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection.Query<QuestionGetManyResponse>(@"EXEC dbo.Question_GetUnanswered");
            }
        }


        public bool QuestionExists(int questionId)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection.QueryFirst<bool>(@"EXEC dbo.Question_Exists @QuestionId = @QuestionId", new { QuestionId = questionId});
            }
        }

        //Write


        public QuestionGetSingleResponse PostQuestion(QuestionPostFullRequest question)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                var questionId = connection.QueryFirst<int>(@"EXEC dbo.Question_Post @Title = @Title,
                                                                                     @Content = @Content,
                                                                                     @UserId = @UserId,
                                                                                     @UserName = @UserName,
                                                                                     @Created = @Created", question);
                return GetQuestion(questionId);
            }
        }

        public QuestionGetSingleResponse PutQuestion(int questionId, QuestionPutRequest question)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                connection.Execute(@"EXEC dbo.Question_Put @QuestionId = @QuestionId, @Title = @Title, @Content = @Content",
                                                    new { QuestionId = questionId, Title = question.Title, Content = question.Content });

                return GetQuestion(questionId);
            }
        }
        public void DeleteQuestion(int questionId)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                connection.Execute(@"EXEC dbo.Question_Delete @QuestionId = @QuestionId", new { QuestionId = questionId });
            }
        }
        public AnswerGetResponse PostAnswer(AnswerPostFullRequest answer)
        {
            using (var connection = new SqlConnection(_db))
            {
                connection.Open();
                return connection.QueryFirst<AnswerGetResponse>(@"EXEC dbo.Answer_Post @QuestionId = @QuestionId, 
                                                                                       @Content = @Content, 
                                                                                       @UserId = @UserId, 
                                                                                       @UserName = @UserName, 
                                                                                       @Created = @Created", answer);
            }
        }
    }
}
